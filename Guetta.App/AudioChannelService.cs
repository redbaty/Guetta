using System.Threading.Tasks;
using Discord;

namespace Guetta.App
{
    public class AudioChannelService
    {
        public AudioChannelService(PlayingService playingService)
        {
            PlayingService = playingService;
        }

        private PlayingService PlayingService { get; }
        
        public async Task Join(IVoiceChannel voiceChannel)
        {
            var audioClient = await voiceChannel.ConnectAsync();
            PlayingService.SetAudioClient(audioClient);
        }
    }
}