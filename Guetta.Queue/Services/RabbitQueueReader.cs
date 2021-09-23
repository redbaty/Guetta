using System;
using System.Threading.Tasks;
using Guetta.App.RabbitMQ;
using Guetta.Queue.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Guetta.Queue.Services
{
    public class RabbitQueueReader : QueueReader<QueueItem>
    {
        private IServiceProvider ServiceProvider { get; }
        
        public RabbitQueueReader(QueueChannelService<QueueItem> queueChannelService, ILogger<RabbitQueueReader> logger, IServiceProvider serviceProvider) : base(queueChannelService, logger)
        {
            ServiceProvider = serviceProvider;
        }

        protected override async Task<bool?> ParseMessage(QueueItem message)
        {
            using var serviceScope = ServiceProvider.CreateScope();
            var queueService = serviceScope.ServiceProvider.GetService<QueueService>();

            if (queueService == null)
            {
                throw new Exception("Queue service is null");
            }

            var index = await queueService.Enqueue(message.VoiceChannelId, message);
            return index is >= 0;
        }
    }
}