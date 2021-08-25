using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;

namespace Guetta.App
{
    public class AudioChannelService
    {
        public AudioChannelService(PlayingService playingService)
        {
            PlayingService = playingService;
        }

        private PlayingService PlayingService { get; }
        
        public async Task Join(DiscordChannel voiceChannel)
        {
            var audioClient = await voiceChannel.ConnectAsync();
            PlayingService.SetAudioClient(audioClient);
        }
    }
}