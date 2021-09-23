using System.ComponentModel.DataAnnotations;

namespace Guetta.Queue.Requests
{
    public class QueueListRequest
    {
        [Required]
        public string VoiceChannelId { get; set; }
    }
}