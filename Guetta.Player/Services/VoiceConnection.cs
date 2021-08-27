using DSharpPlus.VoiceNext;

namespace Guetta.Player.Services
{
    public class VoiceConnection
    {
        public VoiceNextConnection Connection { get; init; }

        public VoiceTransmitSink Sink { get; init; }
    }
}