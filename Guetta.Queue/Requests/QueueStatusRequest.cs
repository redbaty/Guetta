using System.ComponentModel.DataAnnotations;

namespace Guetta.Queue.Requests
{
    public class QueueStatusRequest
    {
        [Required]
        public string VoiceChannelId { get; set; }
    }
}