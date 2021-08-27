using DSharpPlus.VoiceNext;
using Guetta.Player.Requests;

namespace Guetta.Player.Services
{
    public class CurrentlyPlaying
    {
        public string Id { get; init; }
        
        public PlayRequest Request { get; init; }
    }
}