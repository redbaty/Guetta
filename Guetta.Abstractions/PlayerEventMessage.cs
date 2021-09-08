using System.Text.Json.Serialization;
using Guetta.Abstractions.Converters;

namespace Guetta.Abstractions
{
    public enum PlayerEvent
    {
        StartedPlaying = 0,
        EndedPlaying = 1,
        Disconnected = -1
    }
    
    public class PlayerEventMessage
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        [JsonPropertyName("channel")]
        [JsonConverter(typeof(UlongConverter))]
        public ulong Channel { get; init; }
        
        [JsonPropertyName("event")]
        public PlayerEvent Event { get; init; }
    }
}