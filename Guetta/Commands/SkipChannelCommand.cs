using System.Threading.Tasks;
using Discord.WebSocket;
using Guetta.Abstractions;

namespace Guetta.Commands
{
    internal class SkipChannelCommand : IDiscordCommand
    {
        public SkipChannelCommand(QueueService queueService)
        {
            QueueService = queueService;
        }

        private QueueService QueueService { get; }
        
        public async Task ExecuteAsync(SocketMessage message, string[] arguments)
        {
            if (!QueueService.CanSkip())
            {
                await message.Channel.SendMessageAsync("Num pode pular");
                return;
            }
            
            QueueService.Skip();
        }
    }
}