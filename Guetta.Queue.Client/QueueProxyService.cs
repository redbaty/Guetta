using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Guetta.Abstractions;
using Guetta.Queue.Abstractions;
using MessagePack;
using RabbitMQ.Client;

namespace Guetta.Queue.Client
{
    public class QueueProxyService
    {
        public QueueProxyService(HttpClient httpClient, IModel rabbitModel)
        {
            HttpClient = httpClient;
            RabbitModel = rabbitModel;
        }

        private HttpClient HttpClient { get; }

        private IModel RabbitModel { get; }

        public void Enqueue(ulong voiceChannelId, ulong textChannelId, string user, VideoInformation videoInformation)
        {
            RabbitModel.BasicPublish(string.Empty, "play", true, body: MessagePackSerializer.Serialize(new QueueItem
            {
                VideoInformation = videoInformation,
                RequestedByChannel = textChannelId.ToString(),
                RequestedByUser = user,
                VoiceChannelId = voiceChannelId.ToString()
            }));
        }

        public Task<QueueItemWithIndex[]> GetQueueItems(ulong voiceChannelId)
        {
            return HttpClient.GetFromJsonAsync<QueueItemWithIndex[]>($"queue/items?VoiceChannelId={voiceChannelId}");
        }

        public void Skip(ulong voiceChannelId)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new QueueRequest
            {
                VoiceChannelId = voiceChannelId.ToString(),
                RequestType = RequestType.Skip
            }));
            
            RabbitModel.BasicPublish(string.Empty, "queue_command", true, body: body);
        }
    }
}