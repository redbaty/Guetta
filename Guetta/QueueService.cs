using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Guetta.Abstractions;
using Guetta.Extensions;
using Microsoft.Extensions.Logging;

namespace Guetta
{
    internal class QueueService
    {
        public QueueService(YoutubeDlService youtubeDlService, ILogger<QueueService> logger)
        {
            YoutubeDlService = youtubeDlService;
            Logger = logger;
        }

        private IAudioClient AudioClient { get; set; }

        private Queue<QueueItem> Queue { get; } = new();

        private ILogger<QueueService> Logger { get; }

        private YoutubeDlService YoutubeDlService { get; }

        private Task LoopQueue { get; set; }

        private CancellationTokenSource CancellationTokenSource { get; set; }
        
        private QueueItem CurrentItem { get; set; }

        public void SetAudioClient(IAudioClient audioClient)
        {
            AudioClient?.Dispose();
            AudioClient = audioClient;
        }

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
                    
                    CancellationTokenSource = new CancellationTokenSource();
                    await queueItem.Channel.SendMessageAsync(
                            $"Ovo botar a musga que o {queueItem.User.Mention} mandou tocar \"{queueItem.VideoInformation.Title}\"")
                        .DeleteMessageAfter(TimeSpan.FromSeconds(5));
                    queueItem.Playing = true;
                    await Play(queueItem.YoutubeDlInput, CancellationTokenSource.Token)
                        .ContinueWith(_ => Task.CompletedTask);
                    queueItem.Playing = false;
                }

                LoopQueue = null;
            });
        }

        internal IEnumerable<QueueItem> GetQueueItems()
        {
            foreach (var queueItem in Queue)
            {
                yield return queueItem;
            }

            if (CurrentItem != null)
            {
                yield return CurrentItem;
            }
        }

        public bool CanPlay() =>
            AudioClient is {ConnectionState: ConnectionState.Connected};

        public bool CanSkip()
            => CanPlay() && CancellationTokenSource != null;

        public void Enqueue(QueueItem item)
        {
            item.CurrentQueueIndex = Queue.Count + 1;
            Queue.Enqueue(item);
            StartQueueLoop();
        }

        public void Skip()
        {
            if (CancellationTokenSource != null)
            {
                CancellationTokenSource.Cancel();
                CancellationTokenSource = null;
            }
        }

        private async Task Play(string url, CancellationToken cancellationToken)
        {
            await YoutubeDlService.GetAudioStream(url, 0.1, AudioClient, cancellationToken);
        }
    }
}