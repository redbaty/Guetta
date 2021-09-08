using System;
using Guetta.Abstractions.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Guetta.Player.Client
{
    public static class AppExtensions
    {
        public static void AddPlayerClient(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddHttpClient<PlayerProxyService>(c =>
            {
                c.BaseAddress = new Uri(Environment.GetEnvironmentVariable("PLAYER_URL") ??
                                        throw new MissingEnvironmentVariableException("PLAYER_URL"));
            });
        }
    }
}