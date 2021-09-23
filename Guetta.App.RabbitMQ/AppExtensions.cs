using System;
using System.Collections.Generic;
using System.Text.Json;
using Guetta.Abstractions.Exceptions;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Guetta.App.RabbitMQ
{
    public static class AppExtensions
    {
        private static List<Type> RegisteredQueues { get; } = new();

        public static void AddRabbitConnection(this IServiceCollection serviceCollection)
        {
            var rabbitMqConnectionString = Environment.GetEnvironmentVariable("RABBIT_MQ_HOST") ??
                                           throw new MissingEnvironmentVariableException(
                                               "RABBIT_MQ_HOST");
            serviceCollection.AddSingleton(new ConnectionFactory
            {
                HostName = rabbitMqConnectionString,
                UserName = Environment.GetEnvironmentVariable("RABBIT_MQ_USER"),
                Password = Environment.GetEnvironmentVariable("RABBIT_MQ_PASS"),
                DispatchConsumersAsync = true
            });
            serviceCollection.AddSingleton(s => s.GetService<ConnectionFactory>()?.CreateConnection());
            serviceCollection.AddSingleton(s => s.GetService<IConnection>()?.CreateModel());
        }

        public static IServiceCollection AddMessagePackRabbitQueue<TQueueModel>(
            this IServiceCollection serviceCollection, string queueName)
        {
            return AddQueue<TQueueModel>(serviceCollection, queueName, false);
        }
        
        public static IServiceCollection AddJsonRabbitQueue<TQueueModel>(
            this IServiceCollection serviceCollection, string queueName)
        {
            return AddQueue<TQueueModel>(serviceCollection, queueName, true);
        }

        private static IServiceCollection AddQueue<TQueueModel>(IServiceCollection serviceCollection, string queueName,
            bool json)
        {
            serviceCollection.AddSingleton(f =>
            {
                var queueService = new QueueChannelService<TQueueModel>(f.GetService<IModel>(), queueName,
                    f.GetService<ILogger<QueueChannelService<TQueueModel>>>(),
                    json
                        ? bytes => JsonSerializer.Deserialize<TQueueModel>(bytes.Span)
                        : bytes => MessagePackSerializer.Deserialize<TQueueModel>(bytes));
                queueService.Initialize();
                return queueService;
            });

            RegisteredQueues.Add(typeof(QueueChannelService<TQueueModel>));
            return serviceCollection;
        }

        public static void InitializeRabbitQueues(this IServiceProvider serviceProvider)
        {
            foreach (var queue in RegisteredQueues)
            {
                serviceProvider.GetService(queue);
            }
        }
    }
}