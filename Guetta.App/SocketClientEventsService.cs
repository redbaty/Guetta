using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Guetta.App.Extensions;
using Microsoft.Extensions.Logging;

namespace Guetta.App
{
    public class SocketClientEventsService
    {
        public SocketClientEventsService(CommandSolverService commandSolverService, ILogger<SocketClientEventsService> logger)
        {
            CommandSolverService = commandSolverService;
            Logger = logger;
        }

        private CommandSolverService CommandSolverService { get; }

        private ILogger<SocketClientEventsService> Logger { get; }

        private DiscordClient Client { get; set; }

        public void Subscribe(DiscordClient discordSocketClient)
        {
            Unsubscribe();

            Client = discordSocketClient;
            Client.MessageCreated += OnMessageReceived;
            Client.Ready += OnReady;
        }

        private void Unsubscribe()
        {
            if (Client != null)
            {
                Client.MessageCreated -= OnMessageReceived;
                Client.Ready -= OnReady;
            }
        }

        private Task OnReady(DiscordClient sender, ReadyEventArgs readyEventArgs)
        {
            Logger.LogInformation("Ready to go!");
            return Task.CompletedTask;
        }

        private Task OnMessageReceived(DiscordClient sender, MessageCreateEventArgs messageCreateEventArgs)
        {
            var message = messageCreateEventArgs.Message;
            if (message.Content.StartsWith("!"))
            {
                message.DeleteMessageAfter(TimeSpan.FromSeconds(10));
                var commandArguments = message.Content[1..].Split(" ");
                var discordCommand = CommandSolverService.GetCommand(commandArguments.First().ToLower());
                discordCommand.ExecuteAsync(message, commandArguments.Skip(1).ToArray()).ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.LogError(t.Exception, "Error while running command {@Command}", discordCommand);
                    }
                });
            }

            return Task.CompletedTask;
        }
    }
}