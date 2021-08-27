using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Guetta.Abstractions;

namespace Guetta.Services
{
    public class PlayerProxyService
    {
        public PlayerProxyService(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        private HttpClient HttpClient { get; }

        public async Task<string> Play(ulong textChannelId, ulong voiceChannelId, string userMention, string input, VideoInformation videoInformation)
        {
            var request = await HttpClient.PostAsJsonAsync("play", new
            {
                textChannelId = textChannelId.ToString(),
                voiceChannelId = voiceChannelId.ToString(),
                requestedByUser = userMention,
                input,
                videoInformation
            });

            return await request.Content.ReadFromJsonAsync<string>();
        }
        
        public async Task<bool> Skip(ulong voiceChannelId)
        {
            var request = await HttpClient.PostAsJsonAsync("skip", new
            {
                voiceChannelId = voiceChannelId.ToString()
            });

            return await request.Content.ReadFromJsonAsync<bool>();
        }
        
        public async Task<bool> Playing(ulong voiceChannelId)
        {
            var request = await HttpClient.PostAsJsonAsync("playing", new
            {
                voiceChannelId = voiceChannelId.ToString()
            });

            return await request.Content.ReadFromJsonAsync<bool>();
        }
        
        public async Task<bool> SetVolume(ulong voiceChannelId, double volume)
        {
            var request = await HttpClient.PostAsJsonAsync("volume", new
            {
                voiceChannelId = voiceChannelId.ToString(),
                volume
            });

            return await request.Content.ReadFromJsonAsync<bool>();
        }
    }
}