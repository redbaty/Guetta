using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Guetta.Abstractions;
using Guetta.App;
using Guetta.App.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Guetta.Services
{
    public class SocketClientEventsService
    {
        public SocketClientEventsService(CommandSolverService commandSolverService,
            ILogger<SocketClientEventsService> logger, IOptions<CommandOptions> commandOptions)
        {
            CommandSolverService = commandSolverService;
            Logger = logger;
            CommandOptions = commandOptions;
        }

        private CommandSolverService CommandSolverService { get; }

        private ILogger<SocketClientEventsService> Logger { get; }

        private IOptions<CommandOptions> CommandOptions { get; }

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

            if (message.Author.IsBot)
                return Task.CompletedTask;
            
            if (message.Content.StartsWith(CommandOptions.Value.Prefix))
            {
                _ = message.DeleteMessageAfter(TimeSpan.FromSeconds(10));
                var commandArguments = message.Content[1..].Split(" ");
                var discordCommand = CommandSolverService.GetCommand(commandArguments.First().ToLower());

                if (discordCommand != null)
                {
                    _ = message.Channel.TriggerTypingAsync();
                    _ = discordCommand.ExecuteAsync(message, commandArguments.Skip(1).ToArray());
                }
                else
                {
                    _ = message.Channel.SendMessageAsync("Invalid command").DeleteMessageAfter(TimeSpan.FromSeconds(5));
                }
            }

            return Task.CompletedTask;
        }
    }
}