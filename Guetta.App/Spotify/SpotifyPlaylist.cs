using System.Text.Json.Serialization;

namespace Guetta.App.Spotify;

internal record SpotifyPlaylist(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("tracks")] Tracks Tracks
);