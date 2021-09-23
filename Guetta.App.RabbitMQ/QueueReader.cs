using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Guetta.App.RabbitMQ
{
    public interface IQueueReader
    {
        void Initialize();
    }

    public abstract class QueueReader<T> : IQueueReader
    {
        protected QueueReader(QueueChannelService<T> queueChannelService, ILogger<QueueReader<T>> logger)
        {
            QueueChannelService = queueChannelService;
            Logger = logger;
        }

        private QueueChannelService<T> QueueChannelService { get; }

        private ILogger<QueueReader<T>> Logger { get; }
        
        private Task ReaderTask { get; set; }

        public void Initialize()
        {
            if (ReaderTask != null)
            {
                throw new Exception("Already initialized");
            }

            ReaderTask = Task.Run(async () =>
            {
                await foreach (var message in QueueChannelService.MessageReceiver.ReadAllAsync())
                {
                    var ack = await ParseMessage(message.Content).ContinueWith(t =>
                    {
                        if (t.IsCompletedSuccessfully)
                            return t.Result;

                        Logger.LogError(t.Exception, "Exception thrown while trying to parse message. {@Message}",
                            message);
                        return null;
                    });

                    if (ack.HasValue)
                    {
                        if (ack.Value)
                            QueueChannelService.Acknowledged(message.DeliveryTag);
                        else
                            QueueChannelService.Reject(message.DeliveryTag, true);
                    }
                }
            });
        }

        protected abstract Task<bool?> ParseMessage(T message);
    }
}