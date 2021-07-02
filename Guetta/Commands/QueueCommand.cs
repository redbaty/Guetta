using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Guetta.Abstractions;
using Guetta.Extensions;

namespace Guetta.Commands
{
    internal class QueueCommand : IDiscordCommand
    {
        public QueueCommand(QueueService queueService)
        {
            QueueService = queueService;
        }

        private QueueService QueueService { get; }

        public async Task ExecuteAsync(SocketMessage message, string[] arguments)
        {
            var queueMessage = "";

            foreach (var queueItem in QueueService.GetQueueItems().OrderBy(i => i.CurrentQueueIndex))
            {
                queueMessage += $"{queueItem.CurrentQueueIndex + 1}. {queueItem.YoutubeDlInput}{Environment.NewLine}";
            }

            if (string.IsNullOrEmpty(queueMessage))
            {
                await message.Channel
                    .SendMessageAsync("Nenhuma musica na fila")
                    .DeleteMessageAfter(TimeSpan.FromMinutes(1));
                
                return;
            }

            await message.Channel
                .SendMessageAsync(queueMessage)
                .DeleteMessageAfter(TimeSpan.FromMinutes(1));
        }
    }
}