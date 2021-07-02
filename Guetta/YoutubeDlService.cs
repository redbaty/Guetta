using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using Discord.Audio;
using Guetta.Abstractions;
using Microsoft.Extensions.Logging;

namespace Guetta
{
    internal class YoutubeDlService
    {
        public YoutubeDlService(ILogger<YoutubeDlService> logger)
        {
            Logger = logger;
        }

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

            if (rootElement.GetProperty("entries") is {ValueKind: JsonValueKind.Array} entriesElement)
            {
                if (entriesElement.EnumerateArray().SingleOrDefault() is var firstEntryElement)
                {
                    return CreateYoutubeVideoInformation(firstEntryElement);
                }
            }
            
            return CreateYoutubeVideoInformation(rootElement);
        }

        private static VideoInformation CreateYoutubeVideoInformation(JsonElement rootElement)
        {
            return new VideoInformation
            {
                Url = $"https://www.youtube.com/watch?v={rootElement.GetProperty("id").GetString()}",
                Title = rootElement.GetProperty("title").GetString()
            };
        }


        public async Task GetAudioStream(string input, double volume, IAudioClient discordAudioStream,
            CancellationToken cancellationToken)
        {
            await using var discord = discordAudioStream.CreatePCMStream(AudioApplication.Music);

            try
            {
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
                    .WithArguments(youtubeDlArguments, false);

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
                Logger.LogDebug("{@Program} arguments: {@Arguments}", "youtube-dl", youtubeDlArguments);

                var ffmpegCommand = Cli.Wrap("ffmpeg")
                    .WithStandardInputPipe(PipeSource.FromCommand(youtubeDlCommand))
                    .WithStandardOutputPipe(PipeTarget.ToStream(discord, false))
                    .WithStandardErrorPipe(PipeTarget.ToDelegate(s => Logger.LogDebug("FFMpeg message: {@Message}", s)))
                    .WithArguments(ffmpegArguments, false);

                await ffmpegCommand.ExecuteAsync(cancellationToken);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error while downloading from youtube");
            }
            finally
            {
                await discord.FlushAsync(CancellationToken.None);
                await discord.DisposeAsync();
            }
        }
    }
}