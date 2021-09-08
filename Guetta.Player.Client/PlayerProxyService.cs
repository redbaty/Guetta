using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Guetta.Abstractions;

namespace Guetta.Player.Client
{
    public class PlayerProxyService
    {
        public PlayerProxyService(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        private HttpClient HttpClient { get; }

        public async Task<string> Play(ulong voiceChannelId,
            VideoInformation videoInformation, 
            double volume)
        {
            var request = await HttpClient.PostAsJsonAsync("play", new
            {
                voiceChannelId = voiceChannelId.ToString(),
                videoInformation,
                initialVolume = volume
            });

            return request.IsSuccessStatusCode ? await request.Content.ReadAsStringAsync() : null;
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

            return request.IsSuccessStatusCode;
        }
    }
}