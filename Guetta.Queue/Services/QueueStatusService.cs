using System;
using System.Threading.Tasks;
using Guetta.Queue.Abstractions;
using Guetta.Queue.Models;
using Guetta.Queue.Requests;
using MessagePack;
using RedLockNet.SERedis;
using StackExchange.Redis;

namespace Guetta.Queue.Services
{
    public class QueueStatusService
    {
        public QueueStatusService(RedLockFactory redLockFactory, IDatabase database)
        {
            RedLockFactory = redLockFactory;
            Database = database;
        }

        private RedLockFactory RedLockFactory { get; }

        private IDatabase Database { get; }

        private static string GetStatusName(ulong channelId) => $"{channelId}_status";

        public async Task<QueueStatus> GetQueueStatus(ulong channelId)
        {
            var statusName = GetStatusName(channelId);
            await using var @lock = await RedLockFactory.CreateLockAsync(statusName, TimeSpan.FromSeconds(10));
            
            if (!@lock.IsAcquired)
                return null;

            return await GetQueueStatusNoLock(statusName);
        }

        private async Task<QueueStatus> GetQueueStatusNoLock(string statusName)
        {
            var statusRedisValue = await Database.StringGetAsync(statusName);

            if (!statusRedisValue.HasValue)
                return null;
            
            var queueStatus = MessagePackSerializer.Deserialize<QueueStatus>(statusRedisValue);
            return queueStatus;
        }
        
        public async Task<QueueStatus> UpdateQueueStatus(ulong channelId, QueueStatusEnum newStatus, QueueItem newCurrentlyPlaying, string playingId)
        {
            var statusName = GetStatusName(channelId);
            await using var @lock = await RedLockFactory.CreateLockAsync(statusName, TimeSpan.FromSeconds(10));
            
            if (!@lock.IsAcquired)
                return null;

            var newQueueStatus = await GetQueueStatusNoLock(statusName) ?? new QueueStatus{QueueChannel = channelId};
            newQueueStatus.Status = newStatus;
            newQueueStatus.CurrentlyPlaying = newCurrentlyPlaying;
            newQueueStatus.PlayingId = playingId;

            var newQueueStatusSerialized = MessagePackSerializer.Serialize(newQueueStatus);
            await Database.StringSetAsync(statusName, newQueueStatusSerialized);
            return newQueueStatus;
        }
    }
}