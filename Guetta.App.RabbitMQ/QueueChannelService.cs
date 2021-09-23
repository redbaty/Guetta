using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using MessagePack;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Guetta.App.RabbitMQ
{
    public class QueueChannelService<T>
    {
        public QueueChannelService(IModel rabbitModel, string name, ILogger<QueueChannelService<T>> logger, Func<ReadOnlyMemory<byte>, T> deserializer)
        {
            RabbitModel = rabbitModel;
            Name = name;
            Logger = logger;
            Deserializer = deserializer;
        }

        private ILogger<QueueChannelService<T>> Logger { get; }

        private string Name { get; }

        private Channel<QueueMessage<T>> Channel { get; } =
            System.Threading.Channels.Channel.CreateUnbounded<QueueMessage<T>>();

        public ChannelReader<QueueMessage<T>> MessageReceiver => Channel.Reader;

        private IModel RabbitModel { get; }
        
        private Func<ReadOnlyMemory<byte>, T> Deserializer { get; }

        internal void Initialize()
        {
            var consumer = new AsyncEventingBasicConsumer(RabbitModel);
            RabbitModel.BasicConsume(Name, false, consumer);
            consumer.Received += ConsumerOnReceived;
        }

        public void Acknowledged(ulong deliveryTag)
        {
            RabbitModel.BasicAck(deliveryTag, false);
        }

        public void Reject(ulong deliveryTag, bool requeue = false)
        {
            RabbitModel.BasicNack(deliveryTag, false, requeue);
            Logger.LogInformation("Rejecting message {@DeliveryTag}", deliveryTag);
        }

        private async Task ConsumerOnReceived(object sender, BasicDeliverEventArgs e)
        {
            try
            {
                var deserialized = Deserializer.Invoke(e.Body);
                await Channel.Writer.WriteAsync(new QueueMessage<T>
                {
                    Content = deserialized,
                    DeliveryTag = e.DeliveryTag
                });
            }
            catch (MessagePackSerializationException messagePackSerializationException)
            {
                Logger.LogError(messagePackSerializationException, "Failed to parse message {@DeliveryTag}",
                    e.DeliveryTag);
                Reject(e.DeliveryTag);
            }
            catch (Exception exception)
            {
                Logger.LogCritical(exception, "Failed to write message to channel {@DeliveryTag}", e.DeliveryTag);
                throw;
            }
        }
    }
}