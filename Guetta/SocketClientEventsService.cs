using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Guetta.Extensions;
using Microsoft.Extensions.Logging;

namespace Guetta
{
    internal class SocketClientEventsService
    {
        public SocketClientEventsService(CommandSolverService commandSolverService, ILogger<SocketClientEventsService> logger)
        {
            CommandSolverService = commandSolverService;
            Logger = logger;
        }

        private CommandSolverService CommandSolverService { get; }

        private ILogger<SocketClientEventsService> Logger { get; }

        private DiscordSocketClient Client { get; set; }

        public void Subscribe(DiscordSocketClient discordSocketClient)
        {
            Unsubscribe();

            Client = discordSocketClient;
            Client.MessageReceived += OnMessageReceived;
            Client.Ready += OnReady;
            Client.Log += OnLog;
        }

        private Task OnLog(LogMessage message)
        {
            const string template = "[Discord.NET] {@Mensagem}";
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                    Logger.LogCritical(message.Exception, template, message.Message);
                    break;
                case LogSeverity.Error:
                    Logger.LogError(message.Exception, template, message.Message);
                    break;
                case LogSeverity.Warning:
                    Logger.LogWarning(message.Exception, template, message.Message);
                    break;
                case LogSeverity.Info:
                    Logger.LogInformation(message.Exception, template, message.Message);
                    break;
                case LogSeverity.Verbose:
                    Logger.LogDebug(message.Exception, template, message.Message);
                    break;
                case LogSeverity.Debug:
                    Logger.LogDebug(message.Exception, template, message.Message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(message));
            }

            return Task.CompletedTask;
        }

        private void Unsubscribe()
        {
            if (Client != null)
            {
                Client.MessageReceived -= OnMessageReceived;
                Client.Ready -= OnReady;
                Client.Log -= OnLog;
            }
        }

        private Task OnReady()
        {
            Logger.LogInformation("Ready to go!");
            return Task.CompletedTask;
        }

        private Task OnMessageReceived(SocketMessage message)
        {
            if (message.Content.StartsWith("!"))
            {
                message.DeleteMessageAfter(TimeSpan.FromSeconds(10));
                var commandArguments = message.Content[1..].Split(" ");
                var discordCommand = CommandSolverService.GetCommand(commandArguments.First().ToLower());
                discordCommand.ExecuteAsync(message, commandArguments.Skip(1).ToArray());
            }

            return Task.CompletedTask;
        }
    }
}