using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Guetta.Player.Client;
using Guetta.Queue.Abstractions;
using Guetta.Queue.Models;
using MessagePack;
using RedLockNet.SERedis;
using StackExchange.Redis;

namespace Guetta.Queue.Services
{
    public class QueueService
    {
        public QueueService(RedLockFactory redLockFactory, IDatabase database, QueueStatusService queueStatusService,
            PlayerService playerService)
        {
            RedLockFactory = redLockFactory;
            Database = database;
            QueueStatusService = queueStatusService;
            PlayerService = playerService;
        }

        private RedLockFactory RedLockFactory { get; }

        private IDatabase Database { get; }

        private QueueStatusService QueueStatusService { get; }

        private PlayerService PlayerService { get; }

        private static string GetQueueName(string channelId) => $"{channelId}_queue";

        private static string GetQueueCheckingStatusName(string channelId) => $"{channelId}_checking_status";

        internal async Task CheckQueueStatus(string channelId)
        {
            await using var @lock =
                await RedLockFactory.CreateLockAsync(GetQueueCheckingStatusName(channelId), TimeSpan.FromMinutes(1));

            if (!@lock.IsAcquired)
                return;

            var queueStatus = await QueueStatusService.GetQueueStatus(channelId);

            if (queueStatus == null || queueStatus.Status is QueueStatusEnum.Stopped or QueueStatusEnum.ReQueueing)
            {
                var newPlaying = queueStatus is { Status: QueueStatusEnum.ReQueueing }
                    ? queueStatus.CurrentlyPlaying
                    : await Dequeue(channelId);

                if (newPlaying == null)
                {
                    if (queueStatus is not { Status: QueueStatusEnum.Stopped })
                    {
                        await QueueStatusService.UpdateQueueStatus(channelId, QueueStatusEnum.Stopped, null);
                    }

                    return;
                }

                PlayerService.EnqueueToPlay(newPlaying);
                await QueueStatusService.UpdateQueueStatus(channelId, QueueStatusEnum.Playing, newPlaying);
            }
        }

        private async Task<QueueItem> Dequeue(string channelId)
        {
            var queueName = GetQueueName(channelId);
            await using var @lock = await RedLockFactory.CreateLockAsync(queueName, TimeSpan.FromSeconds(10));

            if (!@lock.IsAcquired)
                return null;

            return await DequeueNoLock(queueName);
        }

        private async Task<QueueItem> DequeueNoLock(string queueName)
        {
            var redisValue = await Database.ListRightPopAsync(queueName);

            if (!redisValue.HasValue)
                return null;

            return MessagePackSerializer.Deserialize<QueueItem>(redisValue);
        }

        public async Task<long?> Enqueue(string channelId, QueueItem queueItem)
        {
            var queueName = GetQueueName(channelId);

            await using var @lock = await RedLockFactory.CreateLockAsync(queueName, TimeSpan.FromSeconds(10));

            if (!@lock.IsAcquired)
                return null;

            var serializedRequest = MessagePackSerializer.Serialize(queueItem);
            var pushIndex = await Database.ListLeftPushAsync(queueName, serializedRequest);

            await @lock.DisposeAsync();
            _ = CheckQueueStatus(channelId);

            return pushIndex;
        }

        public async Task Next(string channelId, bool stop = true)
        {
            var queueName = GetQueueName(channelId);

            await using var @lock = await RedLockFactory.CreateLockAsync(queueName, TimeSpan.FromSeconds(10));

            if (!@lock.IsAcquired)
                return;

            if (stop)
            {
                var status = await QueueStatusService.GetQueueStatus(channelId);

                if (status.CurrentlyPlaying != null && status.Status == QueueStatusEnum.Playing)
                    await PlayerService.EnqueueStop(channelId);
            }

            await QueueStatusService.UpdateQueueStatus(channelId, QueueStatusEnum.Stopped, null);
            await @lock.DisposeAsync();
            await CheckQueueStatus(channelId);
        }

        public async IAsyncEnumerable<QueueItemWithIndex> GetQueueItems(string channelId)
        {
            var queueName = GetQueueName(channelId);
            await using var @lock = await RedLockFactory.CreateLockAsync(queueName, TimeSpan.FromSeconds(10));

            if (!@lock.IsAcquired)
                yield break;

            var currentStatus = await QueueStatusService.GetQueueStatus(channelId);
            if (currentStatus is { CurrentlyPlaying: { } })
            {
                yield return new QueueItemWithIndex
                {
                    Playing = true,
                    CurrentQueueIndex = 0,
                    VideoInformation = currentStatus.CurrentlyPlaying.VideoInformation,
                    RequestedByChannel = currentStatus.CurrentlyPlaying.RequestedByChannel,
                    RequestedByUser = currentStatus.CurrentlyPlaying.RequestedByUser,
                    VoiceChannelId = currentStatus.CurrentlyPlaying.VoiceChannelId
                };
            }

            var listValues = await Database.ListRangeAsync(queueName);
            var currentIndex = currentStatus?.CurrentlyPlaying != null ? 1 : 0;

            foreach (var redisValue in listValues)
            {
                var deserialized = MessagePackSerializer.Deserialize<QueueItem>(redisValue);
                yield return new QueueItemWithIndex
                {
                    Playing = currentIndex == 0,
                    CurrentQueueIndex = currentIndex,
                    VideoInformation = deserialized.VideoInformation,
                    RequestedByChannel = deserialized.RequestedByChannel,
                    RequestedByUser = deserialized.RequestedByUser,
                    VoiceChannelId = deserialized.VoiceChannelId
                };

                currentIndex++;
            }
        }
    }
}