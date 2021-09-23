using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Guetta.Abstractions;
using Guetta.App.Extensions;
using Guetta.Localisation;
using Guetta.Queue.Client;

namespace Guetta.Commands
{
    internal class SkipChannelCommand : IDiscordCommand
    {
        public SkipChannelCommand(LocalisationService localisationService, QueueProxyService queueProxyService)
        {
            LocalisationService = localisationService;
            QueueProxyService = queueProxyService;
        }

        private QueueProxyService QueueProxyService { get; }
        
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

            QueueProxyService.Skip(discordMember.VoiceState.Channel.Id);
            await LocalisationService.SendMessageAsync(message.Channel, "SongSkipped", message.Author.Mention)
                .DeleteMessageAfter(TimeSpan.FromSeconds(15));
        }
    }
}