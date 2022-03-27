using System;

namespace Guetta.App;

internal class TypeNotSupportedException : Exception
{
    public TypeNotSupportedException(string type) : base($"This response type for yt-dlp is currently not supported. Please create an issue with the video/playlist URL over at https://github.com/redbaty/Guetta. The type is: {type}")
    {
    }
}