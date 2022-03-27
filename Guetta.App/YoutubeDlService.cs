using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using CliWrap;
using CliWrap.Buffered;
using Guetta.Abstractions;
using Microsoft.Extensions.Logging;



namespace Guetta.App
{
    public class YoutubeDlService
    {
        public YoutubeDlService(ILogger<YoutubeDlService> logger)
        {
            Logger = logger;
        }

        private ILogger<YoutubeDlService> Logger { get; }

        public async IAsyncEnumerable<VideoInformation> GetVideoInformation(string input,
            [EnumeratorCancellation]
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(input))
                yield break;

            if (!Uri.TryCreate(input, UriKind.Absolute, out _))
            {
                input = $"ytsearch:{input}";
            }

            Logger.LogInformation("Getting information for input: {@Input}", input);
            
            var ytDlpCommand = Cli.Wrap("yt-dlp")
                .WithArguments(builder => builder.Add("-J").Add(input));
            var executeBufferedAsync = await ytDlpCommand.ExecuteBufferedAsync(cancellationToken);

            var jsonDocument = JsonDocument.Parse(executeBufferedAsync.StandardOutput);
            var root = jsonDocument.RootElement;
            var returnType = root.GetProperty("_type").GetString();

            Logger.LogDebug("yt-dlp type for input {@Input} is {@Type}", input, returnType);

            switch (returnType)
            {
                case "playlist":
                {
                    var youtubeDlPlaylistInformation = JsonSerializer.Deserialize<YoutubeDlPlaylistInformation>(root.GetRawText()) ?? throw new FailedToGetVideoInformationException();
                    foreach (var youtubeDlVideoInformation in youtubeDlPlaylistInformation.Entries)
                    {
                        yield return youtubeDlVideoInformation.ToVideoInformation();
                    }

                    break;
                }
                case "video":
                {
                    var youtubeDlVideoInformation = JsonSerializer.Deserialize<YoutubeDlVideoInformation>(root.GetRawText()) ?? throw new FailedToGetVideoInformationException();
                    yield return youtubeDlVideoInformation.ToVideoInformation();
                    break;
                }
                default:
                    throw new TypeNotSupportedException(returnType);
            }
        }
    }
}