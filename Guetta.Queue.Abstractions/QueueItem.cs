using System.Text.Json.Serialization;
using Guetta.Abstractions.Converters;
using MessagePack;

namespace Guetta.Queue.Abstractions
{
    [MessagePackObject]
    public class QueueItem : PlayRequest
    {
        [Key(2)]
        [JsonPropertyName("requestedByUser")]
        public string RequestedByUser { get; set; }
        
        [JsonConverter(typeof(UlongConverter))]
        [Key(3)]
        [JsonPropertyName("requestedByChannel")]
        public ulong RequestedByChannel { get; set; }
    }
}