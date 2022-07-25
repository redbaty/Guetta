using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Guetta.Abstractions;
using Guetta.App.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Guetta.App
{
    public class SocketClientEventsService
    {
        public SocketClientEventsService(CommandSolverService commandSolverService, ILogger<SocketClientEventsService> logger, DiscordClient client, IOptions<CommandOptions> commandOptions)
        {
            CommandSolverService = commandSolverService;
            Logger = logger;
            Client = client;
            CommandOptions = commandOptions;
        }

        private CommandSolverService CommandSolverService { get; }

        private ILogger<SocketClientEventsService> Logger { get; }

        private IOptions<CommandOptions> CommandOptions { get; }

        private DiscordClient Client { get; }

        public bool Zombied { get; private set; }

        public void Subscribe()
        {
            Client.MessageCreated += OnMessageReceived;
            Client.Ready += OnReady;
            Client.Zombied += ClientOnZombied;
        }

        private Task ClientOnZombied(DiscordClient sender, ZombiedEventArgs e)
        {
            Zombied = true;
            return Task.CompletedTask;
        }

        private Task OnReady(DiscordClient sender, ReadyEventArgs readyEventArgs)
        {
            Logger.LogInformation("Ready to go!");
            return Task.CompletedTask;
        }

        private async Task OnMessageReceived(DiscordClient sender, MessageCreateEventArgs messageCreateEventArgs)
        {
            var message = messageCreateEventArgs.Message;
            if (message.Content.StartsWith(CommandOptions.Value.Prefix))
            {
                _ = message.DeleteMessageAfter(TimeSpan.FromSeconds(10));
                await CommandSolverService.AddMessageToQueue(message);
            }
        }
    }
}