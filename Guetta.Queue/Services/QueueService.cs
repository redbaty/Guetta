using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Guetta.Player.Client;
using Guetta.Queue.Abstractions;
using Guetta.Queue.Models;
using Guetta.Queue.Requests;
using MessagePack;
using RedLockNet.SERedis;
using StackExchange.Redis;

namespace Guetta.Queue.Services
{
    public class QueueService
    {
        public QueueService(RedLockFactory redLockFactory, IDatabase database, QueueStatusService queueStatusService, PlayerProxyService playerProxyService)
        {
            RedLockFactory = redLockFactory;
            Database = database;
            QueueStatusService = queueStatusService;
            PlayerProxyService = playerProxyService;
        }

        private RedLockFactory RedLockFactory { get; }

        private IDatabase Database { get; }
        
        private QueueStatusService QueueStatusService { get; }
        
        private PlayerProxyService PlayerProxyService { get; }

        private static string GetQueueName(ulong channelId) => $"{channelId}_queue";
        private static string GetQueueCheckingStatusName(ulong channelId) => $"{channelId}_checking_status";

        internal async Task CheckQueueStatus(ulong channelId)
        {
            await using var @lock = await RedLockFactory.CreateLockAsync(GetQueueCheckingStatusName(channelId), TimeSpan.FromMinutes(1));

            if (!@lock.IsAcquired)
                return;

            var queueStatus = await QueueStatusService.GetQueueStatus(channelId);

            if (queueStatus == null || queueStatus.Status is QueueStatusEnum.Stopped or QueueStatusEnum.ReQueueing)
            {
                var newPlaying = queueStatus is { Status: QueueStatusEnum.ReQueueing } ? queueStatus.CurrentlyPlaying : await Dequeue(channelId);

                if (newPlaying == null)
                {
                    if (queueStatus is not { Status: QueueStatusEnum.Stopped })
                    {
                        await QueueStatusService.UpdateQueueStatus(channelId, QueueStatusEnum.Stopped, null, null);
                    }

                    return;
                }

                var playingId = await PlayerProxyService.Play(newPlaying.VoiceChannelId, newPlaying.VideoInformation, 1)
                    .ContinueWith(t => t.IsCompletedSuccessfully ? t.Result : null);

                if (!string.IsNullOrEmpty(playingId))
                    await QueueStatusService.UpdateQueueStatus(channelId, QueueStatusEnum.Playing, newPlaying, playingId);
                else
                    await QueueStatusService.UpdateQueueStatus(channelId, QueueStatusEnum.ReQueueing, newPlaying, playingId);
            }
        }

        private async Task<QueueItem> Dequeue(ulong channelId)
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

        public async Task<long?> Enqueue(ulong channelId, QueueItem queueItem)
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

        public async Task Skip(ulong channelId)
        {
            var queueName = GetQueueName(channelId);

            await using var @lock = await RedLockFactory.CreateLockAsync(queueName, TimeSpan.FromSeconds(10));
            
            if (!@lock.IsAcquired)
                return;
            
            await PlayerProxyService.Skip(channelId);
            await QueueStatusService.UpdateQueueStatus(channelId, QueueStatusEnum.Stopped, null, null);
            await @lock.DisposeAsync();
            await CheckQueueStatus(channelId);
        }

        public async IAsyncEnumerable<QueueItemWithIndex> GetQueueItems(ulong channelId)
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