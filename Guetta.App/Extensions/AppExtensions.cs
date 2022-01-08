﻿using Microsoft.Extensions.DependencyInjection;

namespace Guetta.App.Extensions
{
    public static class AppExtensions
    {
        public static void AddGuettaServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<SocketClientEventsService>();
            serviceCollection.AddSingleton<CommandSolverService>();
            serviceCollection.AddSingleton<GuildContextManager>();
            serviceCollection.AddTransient<YoutubeDlService>();
        }
    }
}