using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Guetta.App.Spotify;

internal record Tracks(
    [property: JsonPropertyName("items")] IReadOnlyList<Item> Items
);