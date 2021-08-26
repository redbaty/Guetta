using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Guetta.Abstractions;
using Microsoft.Extensions.Logging;

namespace Guetta
{
    public class QueueService
    {
        public QueueService(ILogger<QueueService> logger, PlayerProxy playerProxy)
        {
            Logger = logger;
            PlayerProxy = playerProxy;
        }

        private Queue<QueueItem> Queue { get; } = new();

        private ILogger<QueueService> Logger { get; }

        private Task LoopQueue { get; set; }
        
        private PlayerProxy PlayerProxy { get; }

        private QueueItem CurrentItem { get; set; }

        private void ReOrderQueue()
        {
            var index = 1;

            foreach (var queueItem in Queue)
            {
                queueItem.CurrentQueueIndex = index;
                index++;
            }
        }

        private void StartQueueLoop()
        {
            if (LoopQueue != null || Queue.Count <= 0)
                return;

            LoopQueue = Task.Run(async () =>
            {
                while (Queue.TryDequeue(out var queueItem))
                {
                    CurrentItem = queueItem;
                    queueItem.CurrentQueueIndex = 0;
                    ReOrderQueue();

                    Logger.LogInformation("Playing {@Url} requested by {@User}", queueItem.YoutubeDlInput,
                        queueItem.User.Username);
                    

                    queueItem.Playing = true;
                    await PlayerProxy.Play(queueItem.TextChannel.Id, queueItem.VoiceChannel.Id, queueItem.User.Mention, queueItem.YoutubeDlInput, queueItem.VideoInformation)
                        .ContinueWith(t => Task.CompletedTask);
                    
                    while (await PlayerProxy.Playing(queueItem.VoiceChannel.Id))
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }
                    
                    queueItem.Playing = false;
                }

                LoopQueue = null;
            });
        }

        public IEnumerable<QueueItem> GetQueueItems()
        {
            foreach (var queueItem in Queue) yield return queueItem;

            if (CurrentItem != null) yield return CurrentItem;
        }

        public bool CanPlay()
        {
            return  true;
        }

        public bool CanSkip()
        {
            return true;
        }

        public void Enqueue(QueueItem item)
        {
            item.CurrentQueueIndex = Queue.Count + 1;
            Queue.Enqueue(item);
            StartQueueLoop();
        }

        public async Task Skip(ulong channelId)
        {
            await PlayerProxy.Skip(channelId);
        }
    }
}