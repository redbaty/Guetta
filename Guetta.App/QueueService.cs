using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Guetta.Abstractions;
using Guetta.App.Extensions;
using Guetta.Localisation;
using Microsoft.Extensions.Logging;

namespace Guetta.App
{
    public class QueueService
    {
        public QueueService(ILogger<QueueService> logger,
            LocalisationService localisationService, PlayingService playingService)
        {
            Logger = logger;
            LocalisationService = localisationService;
            PlayingService = playingService;
        }

        private Queue<QueueItem> Queue { get; } = new();

        private ILogger<QueueService> Logger { get; }

        private Task LoopQueue { get; set; }
        
        private PlayingService PlayingService { get; }

        private CancellationTokenSource CancellationTokenSource { get; set; }

        private QueueItem CurrentItem { get; set; }

        private LocalisationService LocalisationService { get; }

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

                    CancellationTokenSource?.Dispose();
                    CancellationTokenSource = new CancellationTokenSource();

                    queueItem.Playing = true;
                    await PlayingService.Play(queueItem, CancellationTokenSource.Token);
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
            return PlayingService.AudioClient is { ConnectionState: ConnectionState.Connected };
        }

        public bool CanSkip()
        {
            return CanPlay() && CancellationTokenSource != null;
        }

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
    }
}