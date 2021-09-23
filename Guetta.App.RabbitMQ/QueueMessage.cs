namespace Guetta.App.RabbitMQ
{
    public class QueueMessage<T>
    {
        public ulong DeliveryTag { get; init; }

        public T Content { get; init; }
    }
}