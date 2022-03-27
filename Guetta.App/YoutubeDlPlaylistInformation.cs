using System.Text.Json.Serialization;

namespace Guetta.App;

internal class YoutubeDlPlaylistInformation
{
    [JsonPropertyName("entries")]
    public YoutubeDlVideoInformation[] Entries { get; init; }
}