using System;
using Guetta.Abstractions.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Guetta.App.Redis
{
    public static class AppExtensions
    {
        public static void AddRedisConnection(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton(ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("REDIS") ?? throw new MissingEnvironmentVariableException("REDIS")));
            serviceCollection.AddScoped(i => i.GetService<ConnectionMultiplexer>()!.GetSubscriber());
            serviceCollection.AddScoped(i => i.GetService<ConnectionMultiplexer>()!.GetDatabase());
        }
    }
}