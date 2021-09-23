using System.Text.Json.Serialization;
using MessagePack;

namespace Guetta.Queue.Abstractions
{
    public enum RequestType
    {
        Skip,
        PlayNext
    }
    
    public class QueueRequest
    {
        [JsonPropertyName("voiceChannelId")]
        public string VoiceChannelId { get; set; }
        
        [JsonPropertyName("type")]
        public RequestType RequestType { get; set; }
    }
}