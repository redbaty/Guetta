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
        public SkipChannelCommand(GuildQueue guildQueue, LocalisationService localisationService)
        {
            GuildQueue = guildQueue;
            LocalisationService = localisationService;
        }

        private GuildQueue GuildQueue { get; }
        
        private LocalisationService LocalisationService { get; }
        
        public async Task ExecuteAsync(DiscordMessage message, string[] arguments)
        {
            if (!GuildQueue.CanSkip())
            {
                await LocalisationService.SendMessageAsync(message.Channel, "CantSkip", message.Author.Mention)
                    .DeleteMessageAfter(TimeSpan.FromSeconds(15));
                return;
            }
            
            GuildQueue.Skip();
            await LocalisationService.SendMessageAsync(message.Channel, "SongSkipped", message.Author.Mention)
                .DeleteMessageAfter(TimeSpan.FromSeconds(15));
        }
    }
}