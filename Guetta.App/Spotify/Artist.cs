using System.Text.Json.Serialization;

namespace Guetta.App.Spotify;

internal record Artist(
    [property: JsonPropertyName("name")] string Name
);