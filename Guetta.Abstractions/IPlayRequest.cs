namespace Guetta.Abstractions
{
    public interface IPlayRequest
    {
        ulong VoiceChannelId { get; }

        VideoInformation VideoInformation { get; }
    }
}