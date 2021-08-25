using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Guetta.Abstractions;
using Guetta.App.Extensions;

namespace Guetta.Commands
{
    internal class PingCommand : IDiscordCommand
    {
        public async Task ExecuteAsync(DiscordMessage message, string[] arguments)
        {
            await message.Channel.SendMessageAsync($"{message.Author.Mention} pong")
                .DeleteMessageAfter(TimeSpan.FromSeconds(5));
        }
    }
}