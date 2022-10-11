using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.VoiceNext;
using Guetta.Abstractions;
using Guetta.App.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Guetta.App
{
    public class SocketClientEventsService
    {
        public SocketClientEventsService(CommandSolverService commandSolverService, ILogger<SocketClientEventsService> logger, DiscordClient client, IOptions<CommandOptions> commandOptions, GuildContextManager guildContextManager)
        {
            CommandSolverService = commandSolverService;
            Logger = logger;
            Client = client;
            CommandOptions = commandOptions;
            GuildContextManager = guildContextManager;
        }

        private CommandSolverService CommandSolverService { get; }

        private ILogger<SocketClientEventsService> Logger { get; }

        private IOptions<CommandOptions> CommandOptions { get; }

        private GuildContextManager GuildContextManager { get; }

        private DiscordClient Client { get; }

        public bool Zombied { get; private set; }

        public void Subscribe()
        {
            Client.MessageCreated += OnMessageReceived;
            Client.Ready += OnReady;
            Client.Zombied += ClientOnZombied;
            Client.VoiceStateUpdated += ClientOnVoiceStateUpdated;
        }

        private Task ClientOnVoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs e)
        {
            if (e != null && e.Guild != null)
            {
                var guildContext = GuildContextManager.GetOrDefault(e.Guild.Id);

                if (guildContext != null && e.Before?.Channel != null && e.Before.Channel?.Id == guildContext.Voice.ChannelId)
                {
                    var users = e.Before.Channel?.Users;

                    if (users != null && users.All(i => i.IsBot))
                        if (guildContext is { Voice: { } voice })
                        {
                            guildContext.GuildQueue.Clear();
                        }
                }
            }

            return Task.CompletedTask;
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
            if (!message.Author.IsBot && message.Content.StartsWith(CommandOptions.Value.Prefix))
            {
                _ = message.DeleteMessageAfter(TimeSpan.FromSeconds(10));
                await CommandSolverService.AddMessageToQueue(message);
            }
        }
    }
}