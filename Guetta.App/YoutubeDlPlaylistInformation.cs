using System.Text.Json.Serialization;

namespace Guetta.App;

internal class YoutubeDlPlaylistInformation
{
    [JsonPropertyName("title")]
    public string Title { get; init; }
    
    [JsonPropertyName("entries")]
    public YoutubeDlVideoInformation[] Entries { get; init; }
}