using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Guetta.Abstractions;
using Guetta.App.Extensions;

namespace Guetta.Commands
{
    internal class PingCommand : IDiscordCommand
    {
        public async Task ExecuteAsync(SocketMessage message, string[] arguments)
        {
            await message.Channel.SendMessageAsync($"{message.Author.Mention} pong")
                .DeleteMessageAfter(TimeSpan.FromSeconds(5));
        }
    }
}