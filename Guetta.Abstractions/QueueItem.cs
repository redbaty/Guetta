using Discord;
using Discord.WebSocket;

namespace Guetta.Abstractions
{
    public class QueueItem
    {
        public IUser User { get; init; }

        public ISocketMessageChannel Channel { get; init; }

        public string YoutubeDlInput { get; init; }

        public int CurrentQueueIndex { get; set; }

        public bool Playing { get; set; }

        public VideoInformation VideoInformation { get; init; }
    }
}