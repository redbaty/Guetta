﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Guetta.Abstractions;
using Guetta.App.Extensions;
using Guetta.Localisation;
using Guetta.Queue.Client;
using Microsoft.Extensions.Logging;
using YoutubeExplode;
using YoutubeExplode.Videos;

namespace Guetta.Commands
{
    internal class PlayChannelCommand : IDiscordCommand
    {
        public PlayChannelCommand(LocalisationService localisationService,
            QueueProxyService queueProxyService, ILogger<PlayChannelCommand> logger, YoutubeClient youtubeClient)
        {
            LocalisationService = localisationService;
            QueueProxyService = queueProxyService;
            Logger = logger;
            YoutubeClient = youtubeClient;
        }

        private QueueProxyService QueueProxyService { get; }

        private LocalisationService LocalisationService { get; }

        private ILogger<PlayChannelCommand> Logger { get; }

        private YoutubeClient YoutubeClient { get; }

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

            var searchTerm = arguments.Aggregate((x, y) => $"{x} {y}").Trim();
            VideoInformation videoInformation;

            if (Uri.TryCreate(searchTerm, UriKind.Absolute, out var uri))
            {
                var video = await YoutubeClient.Videos.GetAsync(VideoId.Parse(searchTerm));
                videoInformation = new VideoInformation
                {
                    Title = video.Title,
                    Url = video.Url
                };
            }
            else
            {
                var searchResult = await YoutubeClient.Search.GetVideosAsync(searchTerm).FirstOrDefaultAsync();
                videoInformation = searchResult == null ? null : new VideoInformation
                {
                    Title = searchResult.Title,
                    Url = searchResult.Url
                };
            }

            if (videoInformation != null)
            {
                Logger.LogInformation("Video information for queued gathered. {@VideoInformation}", videoInformation);

                var enqueuedSuccessfully = await QueueProxyService.Enqueue(
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
                    await message.Channel.SendMessageAsync("Failed to enqueue")
                        .DeleteMessageAfter(TimeSpan.FromSeconds(5));
                }
            }
            else
            {
                await message.Channel.SendMessageAsync("No results from that search query")
                    .DeleteMessageAfter(TimeSpan.FromSeconds(5));
            }
        }
    }
}