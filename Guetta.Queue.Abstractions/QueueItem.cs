using System.Text.Json.Serialization;
using MessagePack;

namespace Guetta.Queue.Abstractions
{
    [MessagePackObject]
    public class QueueItem : PlayRequest
    {
        [Key(2)]
        [JsonPropertyName("requestedByUser")]
        public string RequestedByUser { get; set; }
        
        [Key(3)]
        [JsonPropertyName("requestedByChannel")]
        public string RequestedByChannel { get; set; }
    }
}