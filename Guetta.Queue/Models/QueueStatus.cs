using Guetta.Queue.Abstractions;
using Guetta.Queue.Requests;
using MessagePack;

namespace Guetta.Queue.Models
{
    [MessagePackObject]
    public class QueueStatus
    {
        [Key(0)]
        public string QueueChannel { get; set; }

        [Key(1)]
        public QueueStatusEnum Status { get; set; }

        [Key(2)]
        public QueueItem CurrentlyPlaying { get; set; }
    }
}