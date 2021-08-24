using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Guetta.Abstractions;

namespace Guetta.Commands
{
    internal class ClearCommand : IDiscordCommand
    {
        public ClearCommand(DiscordSocketClient discordSocketClient)
        {
            DiscordSocketClient = discordSocketClient;
        }

        private DiscordSocketClient DiscordSocketClient { get; }

        public async Task ExecuteAsync(SocketMessage message, string[] arguments)
        {
            var timeLimit = DateTime.Now.AddDays(-14);
            
            var messagesToDelete = await message.Channel.GetMessagesAsync().Flatten()
                .Where(chatMessage => (chatMessage.Author.Id == DiscordSocketClient.CurrentUser.Id || chatMessage.Content.StartsWith("!")) && chatMessage.CreatedAt >= timeLimit)
                .ToListAsync();

            if (message.Channel is SocketTextChannel socketTextChannel)
                await socketTextChannel.DeleteMessagesAsync(messagesToDelete);
        }
    }
}