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
        public SkipChannelCommand(LocalisationService localisationService, GuildContextManager guildQueue)
        {
            LocalisationService = localisationService;
            GuildContextManager = guildQueue;
        }

        private GuildContextManager GuildContextManager { get; }
        
        private LocalisationService LocalisationService { get; }
        
        public async Task ExecuteAsync(DiscordMessage message, string[] arguments)
        {
            if (!message.Channel.GuildId.HasValue)
            {
                await message.Channel.SendMessageAsync("Invalid guild ID in channel");
                return;
            }

            var guildContext = GuildContextManager.GetOrCreate(message.Channel.GuildId.Value);
            var queue = guildContext.GuildQueue;
            
            if (!queue.CanSkip())
            {
                await LocalisationService.SendMessageAsync(message.Channel, "CantSkip", message.Author.Mention)
                    .DeleteMessageAfter(TimeSpan.FromSeconds(15));
                return;
            }
            
            queue.Skip();
            await LocalisationService.SendMessageAsync(message.Channel, "SongSkipped", message.Author.Mention)
                .DeleteMessageAfter(TimeSpan.FromSeconds(15));
        }
    }
}