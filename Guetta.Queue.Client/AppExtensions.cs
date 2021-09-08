using System;
using Guetta.Abstractions.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Guetta.Queue.Client
{
    public static class AppExtensions
    {
        public static void AddQueueClient(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddHttpClient<QueueProxyService>(c =>
            {
                c.BaseAddress = new Uri(Environment.GetEnvironmentVariable("QUEUE_URL") ??
                                        throw new MissingEnvironmentVariableException("QUEUE_URL"));
            });
        }
    }
}