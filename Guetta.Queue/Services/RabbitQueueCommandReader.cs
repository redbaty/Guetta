using System;
using System.Threading.Tasks;
using Guetta.App.RabbitMQ;
using Guetta.Queue.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Guetta.Queue.Services
{
    public class RabbitQueueCommandReader : QueueReader<QueueRequest>
    {
        private IServiceProvider ServiceProvider { get; }
        
        public RabbitQueueCommandReader(QueueChannelService<QueueRequest> queueChannelService, ILogger<RabbitQueueCommandReader> logger, IServiceProvider serviceProvider) : base(queueChannelService, logger)
        {
            ServiceProvider = serviceProvider;
        }

        protected override async Task<bool?> ParseMessage(QueueRequest message)
        {
            using var serviceScope = ServiceProvider.CreateScope();
            var queueService = serviceScope.ServiceProvider.GetService<QueueService>();

            if (queueService == null)
            {
                throw new Exception("Queue service is null");
            }

            await queueService.Next(message.VoiceChannelId, message.RequestType == RequestType.Skip);
            return true;
        }
    }
}