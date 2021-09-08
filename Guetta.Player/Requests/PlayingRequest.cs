using System.Text.Json.Serialization;
using Guetta.Abstractions.Converters;

namespace Guetta.Player.Requests
{
    public class PlayingRequest
    {
        [JsonConverter(typeof(UlongConverter))]
        public ulong VoiceChannelId { get; set; }
    }
}