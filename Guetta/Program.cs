using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Guetta.Commands;
using Guetta.Exceptions;
using Guetta.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Guetta
{
    internal class Program
    {
        private static async Task Main()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <s:{SourceContext}>{NewLine}{Exception}")
                .CreateLogger();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

            var discordSocketClient = new DiscordSocketClient();
            await discordSocketClient.LoginAsync(TokenType.Bot,
                Environment.GetEnvironmentVariable("TOKEN") ?? throw new MissingEnvironmentVariableException("TOKEN"));
            
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddOptions();
            serviceCollection.AddSingleton(discordSocketClient);
            serviceCollection.AddSingleton<SocketClientEventsService>();
            serviceCollection.AddSingleton<CommandSolverService>();
            serviceCollection.AddSingleton<QueueService>();
            serviceCollection.AddSingleton<AudioChannelService>();
            serviceCollection.AddTransient<YoutubeDlService>();
            serviceCollection.AddLogging(builder => builder.AddSerilog());

            serviceCollection.AddCommand<PingCommand>("ping");
            serviceCollection.AddCommand<JoinChannelCommand>("join");
            serviceCollection.AddCommand<PlayChannelCommand>("play");
            serviceCollection.AddCommand<SkipChannelCommand>("skip");
            serviceCollection.AddCommand<QueueCommand>("queue");

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var socketClientEventsService = serviceProvider.GetService<SocketClientEventsService>();
            socketClientEventsService!.Subscribe(discordSocketClient);
            await discordSocketClient.StartAsync();

            await Task.Delay(-1);
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Logger.Error(e.ExceptionObject as Exception, "Mensagem de erro não tratada");
        }
    }
}