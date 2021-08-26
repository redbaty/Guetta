using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Guetta.Abstractions;
using Guetta.Player.Abstractions;

namespace Guetta
{
    public class PlayerProxy
    {
        public PlayerProxy(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        private HttpClient HttpClient { get; }

        public async Task<bool> Play(ulong textChannelId, ulong voiceChannelId, string userMention, string input, VideoInformation videoInformation)
        {
            var request = await HttpClient.PostAsJsonAsync("play", new
            {
                textChannelId = textChannelId.ToString(),
                voiceChannelId = voiceChannelId.ToString(),
                requestedByUser = userMention,
                input,
                videoInformation
            });

            return await request.Content.ReadFromJsonAsync<bool>();
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
    }
}