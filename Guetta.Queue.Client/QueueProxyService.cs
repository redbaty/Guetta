using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Guetta.Abstractions;
using Guetta.Queue.Abstractions;

namespace Guetta.Queue.Client
{
    public class QueueProxyService
    {
        public QueueProxyService(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        private HttpClient HttpClient { get; }

        public Task<bool> Enqueue(ulong voiceChannelId, ulong textChannelId, string user, VideoInformation videoInformation)
        {
            return HttpClient.PostAsJsonAsync("queue", new
            {
                voiceChannelId = voiceChannelId.ToString(),
                requestedByChannel = textChannelId.ToString(),
                requestedByUser = user,
                videoInformation
            }).ContinueWith(t => t.IsCompletedSuccessfully && t.Result.IsSuccessStatusCode);
        }

        public Task<QueueItemWithIndex[]> GetQueueItems(ulong voiceChannelId)
        {
            return HttpClient.GetFromJsonAsync<QueueItemWithIndex[]>($"queue/items?VoiceChannelId={voiceChannelId}");
        }

        public async Task<bool> Skip(ulong voiceChannelId)
        {
            var request = await HttpClient.PostAsJsonAsync("queue/skip", new
            {
                voiceChannelId = voiceChannelId.ToString()
            });

            return request.IsSuccessStatusCode;
        }
    }
}