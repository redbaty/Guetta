using System.Text.Json.Serialization;

namespace Guetta.App.Spotify;

internal record Item(
    [property: JsonPropertyName("track")] SpotifyTrack Track
);