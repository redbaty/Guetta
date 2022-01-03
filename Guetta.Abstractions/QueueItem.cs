using DSharpPlus.Entities;

namespace Guetta.Abstractions
{
    public class QueueItem
    {
        public DiscordUser User { get; init; }

        public DiscordChannel Channel { get; init; }
        
        public int CurrentQueueIndex { get; set; }

        public bool Playing { get; set; }

        public VideoInformation VideoInformation { get; init; }
    }
}