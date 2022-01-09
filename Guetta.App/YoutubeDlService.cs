using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using DSharpPlus.VoiceNext;
using Guetta.Abstractions;
using Microsoft.Extensions.Logging;
using YoutubeExplode;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace Guetta.App
{
    public class YoutubeDlService
    {
        public YoutubeDlService(ILogger<YoutubeDlService> logger, YoutubeClient youtubeClient)
        {
            Logger = logger;
            YoutubeClient = youtubeClient;
        }

        private ILogger<YoutubeDlService> Logger { get; }

        private YoutubeClient YoutubeClient { get; }

        public async IAsyncEnumerable<VideoInformation> GetVideoInformation(string input,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (Uri.TryCreate(input, UriKind.Absolute, out _))
            {
                if (PlaylistId.TryParse(input) is { } playlistId)
                {
                    await foreach (var video in YoutubeClient.Playlists.GetVideosAsync(playlistId, cancellationToken))
                    {
                        yield return new VideoInformation
                        {
                            Title = video.Title,
                            Url = video.Url
                        };
                    }
                }

                if (VideoId.TryParse(input) is { } videoId)
                {
                    var video = await YoutubeClient.Videos.GetAsync(videoId, cancellationToken);

                    yield return new VideoInformation
                    {
                        Title = video.Title,
                        Url = video.Url
                    };
                }
            }
            else if(!string.IsNullOrEmpty(input))
            {
                await foreach (var searchResult in YoutubeClient.Search.GetVideosAsync(input, cancellationToken))
                {
                    yield return new VideoInformation
                    {
                        Title = searchResult.Title,
                        Url = searchResult.Url
                    };
                    yield break;
                }
            }
        }

        public async Task SendToAudioSink(string input, VoiceTransmitSink currentDiscordStream,
            CancellationToken cancellationToken)
        {
            var youtubeDlArguments = new[]
            {
                "-f bestaudio*[ext=webm][acodec=opus][asr=48000]/ba*[ext=m4a]",
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

                await foreach (var bytes in stream.Reader.ReadAllAsync(cancellationToken))
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