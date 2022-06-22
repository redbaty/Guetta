using System.Text.Json.Serialization;

namespace Guetta.App;

internal class YoutubeDlVideoInformation
{
    [JsonPropertyName("title")]
    public string Title { get; init; }

    [JsonPropertyName("url")]
    public string Url { get; init; }
    
    [JsonPropertyName("webpage_url")]
    public string WebpageUrl { get; init; }
}