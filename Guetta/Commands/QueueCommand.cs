using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Guetta.Abstractions;
using Guetta.App.Extensions;
using Guetta.Localisation;
using Guetta.Queue.Client;
using Guetta.Services;

namespace Guetta.Commands
{
    internal class QueueCommand : IDiscordCommand
    {
        public QueueCommand(LocalisationService localisationService, QueueProxyService queueProxyService)
        {
            LocalisationService = localisationService;
            QueueProxyService = queueProxyService;
        }


        private LocalisationService LocalisationService { get; }

        private QueueProxyService QueueProxyService { get; }

        public async Task ExecuteAsync(DiscordMessage message, string[] arguments)
        {
            if (message.Author is not DiscordMember discordMember)
            {
                await LocalisationService
                    .SendMessageAsync(message.Channel, "NotInChannel", message.Author.Mention)
                    .DeleteMessageAfter(TimeSpan.FromSeconds(5));
                return;
            }

            var queueItems = await QueueProxyService.GetQueueItems(discordMember.VoiceState.Channel.Id)
                .ContinueWith(t => t.IsCompletedSuccessfully ? t.Result : null);

            if (queueItems == null)
            {
                await message.Channel.SendMessageAsync("Could not connect to queue service, please try again later.")
                    .DeleteMessageAfter(TimeSpan.FromSeconds(5));
                return;
            }

            var queueMessage = queueItems.OrderBy(i => i.CurrentQueueIndex)
                .Aggregate("",
                    (current, queueItem) =>
                        current +
                        $"[{queueItem.CurrentQueueIndex + 1}] {queueItem.VideoInformation.Title} (Queued by: {queueItem.RequestedByUser}){Environment.NewLine}");

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