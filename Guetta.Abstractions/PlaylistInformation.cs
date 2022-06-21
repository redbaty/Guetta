using System.Collections.Generic;

namespace Guetta.Abstractions;

public class PlaylistInformation
{
    public string Title { get; init; }
        
    public ICollection<VideoInformation> Videos { get; init; }
}