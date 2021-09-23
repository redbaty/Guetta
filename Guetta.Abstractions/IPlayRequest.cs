namespace Guetta.Abstractions
{
    public interface IPlayRequest
    {
        string VoiceChannelId { get; }

        VideoInformation VideoInformation { get; }
    }
}