using System;
using Guetta.Abstractions.Exceptions;
using Guetta.App.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;

namespace Guetta.Player.Client
{
    public static class AppExtensions
    {
        public static void AddPlayerClient(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddRabbitConnection();
            serviceCollection.AddTransient<PlayerService>();
        }
    }
}