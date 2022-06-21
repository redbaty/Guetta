using System;
using System.Linq;
using System.Text;
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

        private const int Limit = 10;
        
        public async Task ExecuteAsync(DiscordMessage message, string[] arguments)
        {
            if (!message.Channel.GuildId.HasValue)
            {
                await message.Channel.SendMessageAsync("Invalid guild ID in channel");
                return;
            }

            var guildContext = GuildContextManager.GetOrCreate(message.Channel.GuildId.Value);

            var queueMessageBuilder = new StringBuilder();
            var template = LocalisationService.GetMessageTemplate("QueueItem");
            var queueItems = guildContext.GuildQueue.GetQueueItems().ToArray();
            
            foreach (var queueItem in queueItems.Take(Limit))
            {
                queueMessageBuilder.AppendLine(string.Format(template, queueItem.CurrentQueueIndex + 1, queueItem.VideoInformation.Title, queueItem.User.Mention));
            }

            if (queueItems.Length > Limit)
            {
                queueMessageBuilder.AppendLine("And more...");
            }

            var queueMessage = queueMessageBuilder.ToString();
            if (string.IsNullOrEmpty(queueMessage))
            {
                await LocalisationService.SendMessageAsync(message.Channel, "NoSongsInQueue", message.Author.Mention)
                    .DeleteMessageAfter(TimeSpan.FromSeconds(5));

                return;
            }

            await message.Channel
                .SendMessageAsync(queueMessage)
                .DeleteMessageAfter(TimeSpan.FromMinutes(1));
        }
    }
}