using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using DSharpPlus.VoiceNext;
using Guetta.Abstractions;
using Guetta.App.Exceptions;
using Guetta.App.Extensions;
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
            if(string.IsNullOrEmpty(input))
                yield break;
            
            if (!Uri.TryCreate(input, UriKind.Absolute, out _))
            {
                input = $"ytsearch:{input}";
            }
            
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

        public async Task SendToAudioSink(string input, VoiceTransmitSink currentDiscordStream,
            CancellationToken cancellationToken)
        {
            var youtubeDlArguments = new[]
            {
                "-f bestaudio*[ext=webm][acodec=opus][asr=48000]/ba*[ext=m4a]/ba*",
                "--no-continue",
                $"\"{input}\"",
                "-o -"
            };

            var youtubeDlCommand = Cli.Wrap("yt-dlp")
                .WithStandardErrorPipe(PipeTarget.ToDelegate(s =>
                    Logger.LogDebug("Youtube-DL message: {@Message}", s)))
                .WithArguments(youtubeDlArguments, false);

            Logger.LogDebug("{@Program} arguments: {@Arguments}", "yt-dlp", youtubeDlArguments);

            var ffmpegArguments = new[]
            {
                "-hide_banner",
                "-i -",
                "-ac 2",
                "-f s16le",
                "-ar 48000",
                "-"
            };

            Logger.LogDebug("{@Program} arguments: {@Arguments}", "ffmpeg", ffmpegArguments);

            var stream = new ChannelStream();

            var ffmpegCommand = Cli.Wrap("ffmpeg")
                .WithStandardInputPipe(PipeSource.FromCommand(youtubeDlCommand))
                .WithStandardOutputPipe(PipeTarget.ToStream(stream))
                .WithStandardErrorPipe(PipeTarget.ToDelegate(s => Logger.LogDebug("FFMpeg message: {@Message}", s)))
                .WithArguments(ffmpegArguments, false);

            try
            {
                var commandTask = ffmpegCommand.ExecuteAsync(cancellationToken)
                    .Task
                    .ContinueWith(_ => stream.Complete(), cancellationToken);

                await foreach (var bytes in stream.Reader.ReadAllAsync(cancellationToken).ChunkAndMerge(4, cancellationToken))
                {
                    await currentDiscordStream.WriteAsync(bytes, cancellationToken);
                }

                await commandTask;
            }
            finally
            {
                await stream.DisposeAsync();
            }
        }
    }
}