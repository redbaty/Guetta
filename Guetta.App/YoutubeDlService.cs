using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using Guetta.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.IO;

namespace Guetta.App
{
    public class YoutubeDlService
    {
        public YoutubeDlService(ILogger<YoutubeDlService> logger)
        {
            Logger = logger;
        }

        private static RecyclableMemoryStreamManager RecyclableMemoryStreamManager { get; } = new();

        private ILogger<YoutubeDlService> Logger { get; }

        public async Task<VideoInformation> GetVideoInformation(string input)
        {
            var youtubeDlArguments = new[]
            {
                "-J",
                $"\"{input}\""
            };

            var youtubeDlCommand = await Cli.Wrap("youtube-dl")
                .WithArguments(youtubeDlArguments, false)
                .ExecuteBufferedAsync();

            var jsonDocument = JsonDocument.Parse(youtubeDlCommand.StandardOutput);
            var rootElement = jsonDocument.RootElement;

            if (rootElement.TryGetProperty("entries", out var entriesElement) &&
                entriesElement is { ValueKind: JsonValueKind.Array })
            {
                if (entriesElement.EnumerateArray().SingleOrDefault() is var firstEntryElement)
                {
                    return CreateYoutubeVideoInformation(firstEntryElement);
                }
            }

            return CreateYoutubeVideoInformation(rootElement);
        }

        private static VideoInformation CreateYoutubeVideoInformation(JsonElement rootElement) =>
            new()
            {
                Url = $"https://www.youtube.com/watch?v={rootElement.GetProperty("id").GetString()}",
                Title = rootElement.GetProperty("title").GetString()
            };

        public async Task<byte[]> GetYoutubeAudioStream(string input, CancellationToken cancellationToken)
        {
            await using var youtubeAudioStream = RecyclableMemoryStreamManager.GetStream();
            
            var youtubeDlArguments = new[]
            {
                "-f bestaudio",
                "--no-continue",
                $"\"{input}\"",
                "-o -"
            };
            var youtubeDlCommand = Cli.Wrap("youtube-dl")
                .WithStandardErrorPipe(PipeTarget.ToDelegate(s =>
                    Logger.LogDebug("Youtube-DL message: {@Message}", s)))
                .WithStandardOutputPipe(PipeTarget.ToStream(youtubeAudioStream))
                .WithArguments(youtubeDlArguments, false);

            Logger.LogDebug("{@Program} arguments: {@Arguments}", "youtube-dl", youtubeDlArguments);
            await youtubeDlCommand.ExecuteAsync(cancellationToken);
            return youtubeAudioStream.ToArray();
        }

        public async Task<byte[]> GetAudioStream(byte[] inputAudioStream, double volume, CancellationToken cancellationToken)
        {
            await using var convertedAudioStream = RecyclableMemoryStreamManager.GetStream();
            
            var ffmpegArguments = new[]
            {
                "-hide_banner",
                "-i -",
                "-ac 2",
                "-f s16le",
                "-ar 48000",
                $"-filter:a \"volume={volume.ToString(CultureInfo.InvariantCulture)}\"",
                "-"
            };

            Logger.LogDebug("{@Program} arguments: {@Arguments}", "ffmpeg", ffmpegArguments);

            var ffmpegCommand = Cli.Wrap("ffmpeg")
                .WithStandardInputPipe(PipeSource.FromBytes(inputAudioStream))
                .WithStandardOutputPipe(PipeTarget.ToStream(convertedAudioStream))
                .WithStandardErrorPipe(PipeTarget.ToDelegate(s => Logger.LogDebug("FFMpeg message: {@Message}", s)))
                .WithArguments(ffmpegArguments, false);

            await ffmpegCommand.ExecuteAsync(cancellationToken);
            return convertedAudioStream.ToArray();
        }
    }
}