using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Guetta.Abstractions;
using Guetta.App;
using Guetta.App.Extensions;
using Guetta.Localisation;

namespace Guetta.Commands
{
    internal class SkipChannelCommand : IDiscordCommand
    {
        public SkipChannelCommand(QueueService queueService, LocalisationService localisationService)
        {
            QueueService = queueService;
            LocalisationService = localisationService;
        }

        private QueueService QueueService { get; }
        
        private LocalisationService LocalisationService { get; }
        
        public async Task ExecuteAsync(DiscordMessage message, string[] arguments)
        {
            if (!QueueService.CanSkip())
            {
                await LocalisationService.SendMessageAsync(message.Channel, "CantSkip", message.Author.Mention)
                    .DeleteMessageAfter(TimeSpan.FromSeconds(15));
                return;
            }
            
            QueueService.Skip();
            await LocalisationService.SendMessageAsync(message.Channel, "SongSkipped", message.Author.Mention)
                .DeleteMessageAfter(TimeSpan.FromSeconds(15));
        }
    }
}