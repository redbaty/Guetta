using System.Threading.Tasks;
using Discord;

namespace Guetta
{
    internal class AudioChannelService
    {
        public AudioChannelService(QueueService queueService)
        {
            QueueService = queueService;
        }

        private QueueService QueueService { get; }
        
        public async Task Join(IVoiceChannel voiceChannel)
        {
            var connectAsync = await voiceChannel.ConnectAsync();
            QueueService.SetAudioClient(connectAsync);
        }
    }
}