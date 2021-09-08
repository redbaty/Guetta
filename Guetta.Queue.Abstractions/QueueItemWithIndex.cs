using System.Text.Json.Serialization;

namespace Guetta.Queue.Abstractions
{
    public class QueueItemWithIndex : QueueItem
    {
        [JsonPropertyName("currentQueueIndex")]
        public int CurrentQueueIndex { get; set; }

        [JsonPropertyName("playing")]
        public bool Playing { get; set; }
    }
}