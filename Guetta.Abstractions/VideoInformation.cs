using System.Text.Json.Serialization;
using MessagePack;

namespace Guetta.Abstractions
{
    [MessagePackObject]
    public class VideoInformation
    {
        [Key(0)]
        [JsonPropertyName("url")]
        public string Url { get; init; }

        [Key(1)]
        [JsonPropertyName("title")]
        public string Title { get; init; }
    }
}