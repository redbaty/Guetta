using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Guetta.Abstractions;
using Guetta.App;
using Guetta.App.Extensions;
using Guetta.Localisation;
using Guetta.Queue.Client;
using Microsoft.Extensions.Logging;

namespace Guetta.Commands
{
    internal class PlayChannelCommand : IDiscordCommand
    {
        public PlayChannelCommand(YoutubeDlService youtubeDlService, LocalisationService localisationService,
            QueueProxyService queueProxyService, ILogger<PlayChannelCommand> logger)
        {
            YoutubeDlService = youtubeDlService;
            LocalisationService = localisationService;
            QueueProxyService = queueProxyService;
            Logger = logger;
        }

        private QueueProxyService QueueProxyService { get; }

        private YoutubeDlService YoutubeDlService { get; }

        private LocalisationService LocalisationService { get; }

        private ILogger<PlayChannelCommand> Logger { get; }

        public async Task ExecuteAsync(DiscordMessage message, string[] arguments)
        {
            if (arguments.Length < 1)
            {
                await LocalisationService
                    .SendMessageAsync(message.Channel, "InvalidArgument", message.Author.Mention)
                    .DeleteMessageAfter(TimeSpan.FromSeconds(5));
                return;
            }

            if (message.Author is not DiscordMember discordMember || discordMember.VoiceState?.Channel == null)
            {
                await LocalisationService
                    .SendMessageAsync(message.Channel, "NotInChannel", message.Author.Mention)
                    .DeleteMessageAfter(TimeSpan.FromSeconds(5));
                return;
            }

            await message.Channel.TriggerTypingAsync();
            var url = arguments.Last();
            string input;


            input = Uri.TryCreate(url, UriKind.Absolute, out _) ? url : $"ytsearch:{message.Content}";

            var videoInformation = await YoutubeDlService.GetVideoInformation(input, CancellationToken.None)
                .ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully)
                        return t.Result;

                    Logger.LogError(t.Exception, "Failed to obtain video information");
                    return null;
                });

            Logger.LogInformation("Video information for queued gathered. {@VideoInformation}", videoInformation);

            var enqueuedSuccessfully = videoInformation != null && await QueueProxyService.Enqueue(
                discordMember.VoiceState.Channel.Id, message.ChannelId,
                message.Author.Mention, videoInformation);

            if (enqueuedSuccessfully)
            {
                await LocalisationService
                    .SendMessageAsync(message.Channel, "SongQueued", message.Author.Mention, videoInformation.Title)
                    .DeleteMessageAfter(TimeSpan.FromSeconds(5));
            }
            else
            {
                await message.Channel.SendMessageAsync("Failed to enqueue").DeleteMessageAfter(TimeSpan.FromSeconds(5));
            }
        }
    }
}