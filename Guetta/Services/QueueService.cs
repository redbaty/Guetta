using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Guetta.Abstractions;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Guetta.Services
{
    public class QueueService
    {
        public QueueService(ILogger<QueueService> logger, PlayerProxyService playerProxyService, ISubscriber subscriber,
            IDatabase database)
        {
            Logger = logger;
            PlayerProxyService = playerProxyService;
            Subscriber = subscriber;
            Database = database;
        }

        private Queue<QueueItem> Queue { get; } = new();

        private ILogger<QueueService> Logger { get; }

        private Task LoopQueue { get; set; }

        private PlayerProxyService PlayerProxyService { get; }

        private ISubscriber Subscriber { get; }

        private IDatabase Database { get; }

        private QueueItem CurrentItem { get; set; }

        private TaskCompletionSource<string> WaitPlay { get; set; }

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

                    Logger.LogInformation("Playing {@Url} requested by {@User}", queueItem.VideoInformation.Url,
                        queueItem.User.Username);


                    queueItem.Playing = true;

                    WaitPlay = new TaskCompletionSource<string>();

                    var channelVolume = await Database.HashGetAsync(queueItem.VoiceChannel.Id.ToString(), "volume");

                    var id = await PlayerProxyService
                        .Play(queueItem.TextChannel.Id, queueItem.VoiceChannel.Id, queueItem.User.Mention,
                            queueItem.VideoInformation, channelVolume.HasValue ? (double)channelVolume : 1)
                        .ContinueWith(t => t.IsCompletedSuccessfully ? t.Result : null);

                    if (id != null)
                    {
                        var channel = $"{id}:ended";
                        await Subscriber.SubscribeAsync(channel,
                            (_, value) => { WaitPlay.SetResult(value); });

                        await WaitPlay.Task;
                        await Subscriber.UnsubscribeAsync(channel);
                    }

                    queueItem.Playing = false;
                    CurrentItem = null;
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
            return true;
        }

        public Task<bool> CanSkip(ulong channel)
        {
            return PlayerProxyService.Playing(channel);
        }

        public void Enqueue(QueueItem item)
        {
            item.CurrentQueueIndex = Queue.Count + 1;
            Queue.Enqueue(item);
            StartQueueLoop();
        }

        public async Task Skip(ulong channelId)
        {
            await PlayerProxyService.Skip(channelId);
            StartQueueLoop();
        }
    }
}