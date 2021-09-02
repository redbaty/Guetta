using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using DSharpPlus.VoiceNext;
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

        public async Task<VideoInformation> GetVideoInformation(string input, CancellationToken cancellationToken)
        {
            var youtubeDlArguments = new[]
            {
                "-J",
                $"\"{input}\""
            };

            var youtubeDlCommand = await Cli.Wrap("youtube-dl")
                .WithArguments(youtubeDlArguments, false)
                .ExecuteBufferedAsync(Encoding.UTF8, cancellationToken);

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
        
        public async Task SendToAudioSink(string input, VoiceTransmitSink currentDiscordStream,
            CancellationToken cancellationToken)
        {
            var youtubeDlArguments = new[]
            {
                "-f bestaudio[ext=webm+acodec=opus+asr=48000]/bestaudio",
                "-r 100K",
                "--no-continue",
                $"\"{input}\"",
                "-o -"
            };
            
            var youtubeDlCommand = Cli.Wrap("youtube-dl")
                .WithStandardErrorPipe(PipeTarget.ToDelegate(s =>
                    Logger.LogDebug("Youtube-DL message: {@Message}", s)))
                .WithArguments(youtubeDlArguments, false);

            Logger.LogDebug("{@Program} arguments: {@Arguments}", "youtube-dl", youtubeDlArguments);

            var ffmpegArguments = new[]
            {
                "-hide_banner",
                "-hwaccel auto",
                "-i -",
                "-ac 2",
                "-f s16le",
                "-ar 48000",
                "-"
            };

            Logger.LogDebug("{@Program} arguments: {@Arguments}", "ffmpeg", ffmpegArguments);

            await using var stream = new VoiceTransmitSinkStream(currentDiscordStream);

            var ffmpegCommand = Cli.Wrap("ffmpeg")
                .WithStandardInputPipe(PipeSource.FromCommand(youtubeDlCommand))
                .WithStandardOutputPipe(PipeTarget.ToStream(stream, false))
                .WithStandardErrorPipe(PipeTarget.ToDelegate(s => Logger.LogDebug("FFMpeg message: {@Message}", s)))
                .WithArguments(ffmpegArguments, false);

            await ffmpegCommand.ExecuteAsync(cancellationToken);
        }
    }
}