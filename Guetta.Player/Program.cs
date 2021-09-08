using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.VoiceNext;
using Guetta.Abstractions.Exceptions;
using Guetta.App;
using Guetta.App.Extensions;
using Guetta.Localisation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Extensions.Logging;

namespace Guetta.Player
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(
                    outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <s:{SourceContext}>{NewLine}{Exception}")
                .CreateLogger();

            var discordSocketClient = new DiscordClient(new DiscordConfiguration
            {
                Token = Environment.GetEnvironmentVariable("TOKEN") ??
                        throw new MissingEnvironmentVariableException("TOKEN"),
                TokenType = TokenType.Bot,
                LoggerFactory = new SerilogLoggerFactory()
            });
            discordSocketClient.UseVoiceNext();
            await discordSocketClient.ConnectAsync();
            var host = CreateHostBuilder(args)
                .ConfigureServices(s =>
                {
                    s.AddSingleton(discordSocketClient);
                    s.AddGuettaServices();
                    s.AddGuettaLocalisation();
                })
                .Build();
            await host.Services.ValidateAndConfigureLocalisation();

            await host.RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}