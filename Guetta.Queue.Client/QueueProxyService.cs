using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Guetta.Abstractions;
using Guetta.Queue.Abstractions;
using Microsoft.Extensions.Logging;

namespace Guetta.Queue.Client
{
    public class QueueProxyService
    {
        public QueueProxyService(HttpClient httpClient, ILogger<QueueProxyService> logger)
        {
            HttpClient = httpClient;
            Logger = logger;
        }

        private HttpClient HttpClient { get; }
        
        private ILogger<QueueProxyService> Logger { get; }

        public async Task<bool> Enqueue(ulong voiceChannelId, ulong textChannelId, string user, VideoInformation videoInformation)
        {
            return await HttpClient.PostAsJsonAsync("queue", new
            {
                voiceChannelId = voiceChannelId.ToString(),
                requestedByChannel = textChannelId.ToString(),
                requestedByUser = user,
                videoInformation
            }).ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully && t.Result.IsSuccessStatusCode)
                {
                    Logger.LogInformation("Enqueued new song. {@VideoInformation}", videoInformation);
                    return true;
                }
                
                Logger.LogError("Failed to enqueue new song. {@VideoInformation}", videoInformation);
                return false;
            });
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