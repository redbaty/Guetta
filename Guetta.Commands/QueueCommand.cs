using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Guetta.Abstractions;
using Guetta.App;
using Guetta.App.Extensions;
using Guetta.Localisation;

namespace Guetta.Commands
{
    internal class QueueCommand : IDiscordCommand
    {
        public QueueCommand(LocalisationService localisationService, GuildContextManager guildContextManager)
        {
            LocalisationService = localisationService;
            GuildContextManager = guildContextManager;
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

            var queueMessage = "";

            foreach (var queueItem in guildContext.GuildQueue.GetQueueItems().OrderBy(i => i.CurrentQueueIndex))
            {
                queueMessage += $"[{queueItem.CurrentQueueIndex + 1}] {queueItem.VideoInformation.Title} (Queued by: {queueItem.User.Mention}){Environment.NewLine}";
            }

            if (string.IsNullOrEmpty(queueMessage))
            {
                await LocalisationService.SendMessageAsync(message.Channel, "NoSongsInQueue", message.Author.Mention)
                    .DeleteMessageAfter(TimeSpan.FromSeconds(15));

                return;
            }

            await message.Channel
                .SendMessageAsync(queueMessage)
                .DeleteMessageAfter(TimeSpan.FromMinutes(1));
        }
    }
}