using MessagePack;

namespace Guetta.Abstractions
{
    [MessagePackObject]
    public class VideoInformation
    {
        [Key(0)]
        public string Url { get; init; }

        [Key(1)]
        public string Title { get; init; }
    }
}