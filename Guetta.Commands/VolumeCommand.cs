using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Guetta.Abstractions;
using Guetta.App;
using Guetta.App.Extensions;

namespace Guetta.Commands
{
    internal class VolumeCommand : IDiscordCommand
    {
        public VolumeCommand(PlayingService playingService)
        {
            PlayingService = playingService;
        }

        private PlayingService PlayingService { get; }
        
        public async Task ExecuteAsync(DiscordMessage message, string[] arguments)
        {
            if (int.TryParse(arguments[0], out var volume))
            {
                await message.Channel.TriggerTypingAsync();
                await PlayingService.ChangeVolume(volume / 100f, CancellationToken.None);
                await message.Channel.SendMessageAsync("Volume alterado queridão").DeleteMessageAfter(TimeSpan.FromSeconds(5));
            }
        }
    }
}