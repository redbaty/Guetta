using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Guetta.Abstractions;
using Guetta.App.Extensions;
using Guetta.Localisation;
using Guetta.Services;

namespace Guetta.Commands
{
    internal class VolumeCommand : IDiscordCommand
    {
        public VolumeCommand(PlayerProxyService playerProxyService, LocalisationService localisationService)
        {
            PlayerProxyService = playerProxyService;
            LocalisationService = localisationService;
        }

        private PlayerProxyService PlayerProxyService { get; }

        private LocalisationService LocalisationService { get; }

        public async Task ExecuteAsync(DiscordMessage message, string[] arguments)
        {
            if (message.Author is not DiscordMember discordMember)
            {
                await LocalisationService
                    .SendMessageAsync(message.Channel, "NotInChannel", message.Author.Mention)
                    .DeleteMessageAfter(TimeSpan.FromSeconds(5));
                return;
            }

            if (int.TryParse(arguments[0], out var volume))
            {
                await message.Channel.TriggerTypingAsync();
                var mensagem = await PlayerProxyService.SetVolume(discordMember.VoiceState.Channel.Id, volume / 100f)
                    .ContinueWith(t => t.IsCompletedSuccessfully && t.Result ? "Volume alterado queridão" : "Deu ruim pra alterar o volume");
                await message.Channel.SendMessageAsync(mensagem)
                    .DeleteMessageAfter(TimeSpan.FromSeconds(5));
            }
        }
    }
}