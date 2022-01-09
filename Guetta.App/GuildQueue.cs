using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Guetta.Abstractions;
using Guetta.Localisation;
using Microsoft.Extensions.Logging;

namespace Guetta.App
{
    public class GuildQueue : IGuildItem
    {
        public GuildQueue(ILogger<GuildQueue> logger,
            LocalisationService localisationService, Voice voice, ulong guildId)
        {
            Logger = logger;
            LocalisationService = localisationService;
            Voice = voice;
            GuildId = guildId;
        }

        public ulong GuildId { get; }

        private Queue<QueueItem> Queue { get; } = new();

        private ILogger<GuildQueue> Logger { get; }

        private Task LoopQueue { get; set; }

        private Voice Voice { get; }

        private CancellationTokenSource CancellationTokenSource { get; set; }

        private QueueItem CurrentItem => Voice.CurrentItem;

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
                    queueItem.CurrentQueueIndex = 0;
                    ReOrderQueue();

                    Logger.LogInformation("Playing {@Title} requested by {@User}", queueItem.VideoInformation.Title, queueItem.User.Username);

                    CancellationTokenSource?.Dispose();
                    CancellationTokenSource = new CancellationTokenSource();

                    queueItem.Playing = true;
                    await Voice.Play(queueItem, CancellationTokenSource.Token);
                    queueItem.Playing = false;
                }

                LoopQueue = null;
            });
        }

        public IEnumerable<QueueItem> GetQueueItems()
        {
            foreach (var queueItem in Queue.OrderBy(i => i.CurrentQueueIndex)) yield return queueItem;

            if (CurrentItem != null) yield return CurrentItem;
        }

        public bool CanPlay()
        {
            return Voice.AudioClient is { };
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