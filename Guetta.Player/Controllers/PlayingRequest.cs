using System.Text.Json.Serialization;

namespace Guetta.Player.Controllers
{
    public class PlayingRequest
    {
        [JsonConverter(typeof(UlongConverter))]
        public ulong VoiceChannelId { get; set; }
    }
}