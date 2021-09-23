using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Guetta.Abstractions;
using Guetta.App.Extensions;
using Guetta.Localisation;
using Guetta.Player.Client;
using Guetta.Services;
using StackExchange.Redis;

namespace Guetta.Commands
{
    internal class VolumeCommand : IDiscordCommand
    {
        public VolumeCommand(LocalisationService localisationService, IDatabase database, PlayerService playerService)
        {
            LocalisationService = localisationService;
            Database = database;
            PlayerService = playerService;
        }

        private LocalisationService LocalisationService { get; }
        
        private PlayerService PlayerService { get; }
        
        private IDatabase Database { get; }

        public async Task ExecuteAsync(DiscordMessage message, string[] arguments)
        {
            if (message.Author is not DiscordMember discordMember)
            {
                await LocalisationService
                    .SendMessageAsync(message.Channel, "NotInChannel", message.Author.Mention)
                    .DeleteMessageAfter(TimeSpan.FromSeconds(5));
                return;
            }

            if (arguments.Length > 0 && int.TryParse(arguments[0], out var newVolume))
            {
                await message.Channel.TriggerTypingAsync();

                var volume = newVolume / 100d;
                await PlayerService.EnqueueVolumeChange(discordMember.VoiceState.Channel.Id, volume);
                await message.Channel.SendMessageAsync("Volume alterado queridão")
                    .DeleteMessageAfter(TimeSpan.FromSeconds(5));
            }
            else
            {
                var currentVolume = await Database.HashGetAsync(discordMember.VoiceState.Channel.Id.ToString(), "volume");
                var messageContent = currentVolume.HasValue ? $"Current volume is set at {(double) currentVolume:P}" : "Current volume is set at 100%";
                
                await message.Channel
                    .SendMessageAsync(messageContent)
                    .DeleteMessageAfter(TimeSpan.FromSeconds(5));
            }
        }
    }
}