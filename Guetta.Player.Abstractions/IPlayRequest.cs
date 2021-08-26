using Guetta.Abstractions;

namespace Guetta.Player.Abstractions
{
    public interface IPlayRequest
    {
        public ulong TextChannelId { get; }
        
        public ulong VoiceChannelId { get; }
        
        public string RequestedByUser { get; }
        
        public string Input { get; }
        
        public VideoInformation VideoInformation { get; }
    }
}