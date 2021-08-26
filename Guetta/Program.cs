using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.VoiceNext;
using Guetta.App;
using Guetta.App.Extensions;
using Guetta.Commands.Extensions;
using Guetta.Exceptions;
using Guetta.Localisation;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Extensions.Logging;

namespace Guetta
{
    internal class Program
    {
        private static async Task Main()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(
                    outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <s:{SourceContext}>{NewLine}{Exception}")
                .CreateLogger();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

            var discordSocketClient = new DiscordClient(new DiscordConfiguration
            {
                Token =  Environment.GetEnvironmentVariable("TOKEN") ?? throw new MissingEnvironmentVariableException("TOKEN"),
                TokenType = TokenType.Bot,
                LoggerFactory = new SerilogLoggerFactory()
            });
            discordSocketClient.UseVoiceNext();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddOptions();
            serviceCollection.AddSingleton(discordSocketClient);
            serviceCollection.AddGuettaServices();
            serviceCollection.AddGuettaCommands();
            serviceCollection.AddGuettaLocalisation();
            serviceCollection.WithPrefix("!");
            serviceCollection.AddLogging(builder => builder.AddSerilog());
            
            var serviceProvider = serviceCollection.BuildServiceProvider();
            await serviceProvider.ValidateAndConfigureLocalisation();

            var socketClientEventsService = serviceProvider.GetService<SocketClientEventsService>();
            socketClientEventsService!.Subscribe(discordSocketClient);
            await discordSocketClient.ConnectAsync();

            await Task.Delay(-1);
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Logger.Error(e.ExceptionObject as Exception, "Mensagem de erro não tratada");
        }
    }
}