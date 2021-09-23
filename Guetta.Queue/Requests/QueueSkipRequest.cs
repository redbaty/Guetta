using System.ComponentModel.DataAnnotations;

namespace Guetta.Queue.Requests
{
    public class QueueSkipRequest
    {
        [Required]
        public string VoiceChannelId { get; set; }
    }
}