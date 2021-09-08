using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace Guetta.Queue.Extensions
{
    public static class AppExtensions
    {
        public static void AddRedLock(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton(f => RedLockFactory.Create(new List<RedLockMultiplexer>
            {
                f.GetService<ConnectionMultiplexer>()
            }));
        }
    }
}