using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Guetta.Abstractions;
using Guetta.App.Extensions;
using Guetta.Localisation;
using Guetta.Services;
using StackExchange.Redis;

namespace Guetta.Commands
{
    internal class VolumeCommand : IDiscordCommand
    {
        public VolumeCommand(PlayerProxyService playerProxyService, LocalisationService localisationService, IDatabase database)
        {
            PlayerProxyService = playerProxyService;
            LocalisationService = localisationService;
            Database = database;
        }

        private PlayerProxyService PlayerProxyService { get; }

        private LocalisationService LocalisationService { get; }
        
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
                var mensagem = await PlayerProxyService
                    .SetVolume(discordMember.VoiceState.Channel.Id, volume)
                    .ContinueWith(t => t.IsCompletedSuccessfully && t.Result);

                if (mensagem)
                    await Database.HashSetAsync(discordMember.VoiceState.Channel.Id.ToString(), "volume", volume);

                await message.Channel.SendMessageAsync(mensagem ? "Volume alterado queridão" : "Deu ruim pra alterar o volume")
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