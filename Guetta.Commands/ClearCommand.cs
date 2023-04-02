using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Guetta.Abstractions;
using Microsoft.Extensions.Options;

namespace Guetta.Commands
{
    internal class ClearCommand : IDiscordCommand
    {
        public ClearCommand(DiscordClient discordSocketClient, IOptions<CommandOptions> commandOptions)
        {
            DiscordSocketClient = discordSocketClient;
            CommandOptions = commandOptions;
        }

        private DiscordClient DiscordSocketClient { get; }
        
        private IOptions<CommandOptions> CommandOptions { get; }

        public async Task ExecuteAsync(DiscordMessage message, string[] arguments)
        {
            var timeLimit = DateTime.Now.AddDays(-14);
            var messages = await message.Channel.GetMessagesAsync();
            var messagesToDelete = messages
                .Where(chatMessage => (chatMessage.Author.Id == DiscordSocketClient.CurrentUser.Id || chatMessage.Content.StartsWith(CommandOptions.Value.Prefix) && chatMessage.CreationTimestamp >= timeLimit)
                .ToList();

            await message.Channel.DeleteMessagesAsync(messagesToDelete);
        }
    }
}