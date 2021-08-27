using System;
using Guetta.App.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Guetta.App.Extensions
{
    public static class AppExtensions
    {
        public static void AddGuettaServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<YoutubeDlService>();
        }

        public static void AddRedisConnection(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton(ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("REDIS") ?? throw new MissingEnvironmentVariableException("REDIS")));
            serviceCollection.AddScoped(i => i.GetService<ConnectionMultiplexer>()!.GetSubscriber());
            serviceCollection.AddScoped(i => i.GetService<ConnectionMultiplexer>()!.GetDatabase());
        }
    }
}