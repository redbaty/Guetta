using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Guetta.Abstractions;

namespace Guetta.Commands
{
    internal class ClearCommand : IDiscordCommand
    {
        public ClearCommand(DiscordClient discordSocketClient)
        {
            DiscordSocketClient = discordSocketClient;
        }

        private DiscordClient DiscordSocketClient { get; }

        public async Task ExecuteAsync(DiscordMessage message, string[] arguments)
        {
            var timeLimit = DateTime.Now.AddDays(-14);
            var messages = await message.Channel.GetMessagesAsync();
            var messagesToDelete = messages
                .Where(chatMessage => (chatMessage.Author.Id == DiscordSocketClient.CurrentUser.Id || chatMessage.Content.StartsWith("!")) && chatMessage.CreationTimestamp >= timeLimit)
                .ToList();

            await message.Channel.DeleteMessagesAsync(messagesToDelete);
        }
    }
}