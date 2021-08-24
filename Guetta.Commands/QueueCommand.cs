using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Guetta.Abstractions;
using Guetta.App;
using Guetta.App.Extensions;
using Guetta.Localisation;

namespace Guetta.Commands
{
    internal class QueueCommand : IDiscordCommand
    {
        public QueueCommand(QueueService queueService, LocalisationService localisationService)
        {
            QueueService = queueService;
            LocalisationService = localisationService;
        }

        private QueueService QueueService { get; }
        
        private LocalisationService LocalisationService { get; }

        public async Task ExecuteAsync(SocketMessage message, string[] arguments)
        {
            var queueMessage = "";

            foreach (var queueItem in QueueService.GetQueueItems().OrderBy(i => i.CurrentQueueIndex))
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