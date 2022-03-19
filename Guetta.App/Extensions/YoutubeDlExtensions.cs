using Guetta.Abstractions;

namespace Guetta.App.Extensions;

internal static class YoutubeDlExtensions
{
    public static VideoInformation ToVideoInformation(this YoutubeDlVideoInformation youtubeDlVideoInformation) => new VideoInformation
    {
        Title = youtubeDlVideoInformation.Title,
        Url = youtubeDlVideoInformation.Url
    };
}