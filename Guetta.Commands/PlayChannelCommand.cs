using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Guetta.Abstractions;
using Guetta.App;
using Guetta.App.Extensions;
using Guetta.Localisation;

namespace Guetta.Commands
{
    internal class PlayChannelCommand : IDiscordCommand
    {
        public PlayChannelCommand(YoutubeDlService youtubeDlService, LocalisationService localisationService, GuildContextManager guildContextManager, DiscordClient discordClient)
        {
            YoutubeDlService = youtubeDlService;
            LocalisationService = localisationService;
            GuildContextManager = guildContextManager;
            DiscordClient = discordClient;
        }

        private GuildContextManager GuildContextManager { get; }

        private YoutubeDlService YoutubeDlService { get; }

        private LocalisationService LocalisationService { get; }

        private DiscordClient DiscordClient { get; }

        public async Task ExecuteAsync(DiscordMessage message, string[] arguments)
        {
            if (arguments.Length < 1)
            {
                await LocalisationService
                    .SendMessageAsync(message.Channel, "InvalidArgument", message.Author.Mention)
                    .DeleteMessageAfter(TimeSpan.FromSeconds(5));
                return;
            }

            if (!message.Channel.GuildId.HasValue)
            {
                await message.Channel.SendMessageAsync("Invalid guild ID in channel");
                return;
            }

            var guildContext = GuildContextManager.GetOrCreate(message.Channel.GuildId.Value);
            var queue = guildContext.GuildQueue;

            if (!queue.CanPlay())
            {
                if (message.Author is DiscordMember { VoiceState: { Channel: { } } } discordMember)
                {
                    await guildContext.Voice.Join(discordMember.VoiceState.Channel);
                }
                else
                {
                    await LocalisationService
                        .SendMessageAsync(message.Channel, "NotInChannel", message.Author.Mention)
                        .DeleteMessageAfter(TimeSpan.FromSeconds(5));
                    return;
                }
            }

            await message.Channel.TriggerTypingAsync();
            var url = arguments.Aggregate((x, y) => $"{x} {y}");

            var playlistInformation = await YoutubeDlService.GetVideoInformation(url);
            var queueItems = playlistInformation?.Videos
                .Select(i => new QueueItem
                {
                    User = message.Author,
                    Channel = message.Channel,
                    VideoInformation = i
                }).ToArray();

            if (queueItems is { Length: > 1 })
            {
                var positiveEmoji = DiscordEmoji.FromName(DiscordClient, ":white_check_mark:");
                var negativeEmoji = DiscordEmoji.FromName(DiscordClient, ":negative_squared_cross_mark:");
                var respondAsync = await message.RespondAsync(string.Format(LocalisationService.GetMessageTemplate("MultipleSongsConfirmation"), queueItems.Length, positiveEmoji, negativeEmoji));
                var confirmPlaylistQueue = await respondAsync.Ask(message.Author, positiveEmoji, negativeEmoji, TimeSpan.FromSeconds(10));
                await respondAsync.DeleteAsync();

                if (!confirmPlaylistQueue)
                {
                    await LocalisationService.ReplyMessageAsync(message, "MultipleSongsConfirmationCanceled").DeleteMessageAfter(TimeSpan.FromSeconds(5));
                    return;
                }
            }

            if (queueItems != null)
                foreach (var queueItem in queueItems)
                    queue.Enqueue(queueItem);

            if (queueItems is { Length: > 0 } && string.IsNullOrEmpty(playlistInformation.Title))
            {
                await LocalisationService
                    .ReplyMessageAsync(message, "SongQueued", message.Author.Mention, string.Empty)
                    .DeleteMessageAfter(TimeSpan.FromSeconds(5));
            }
            else if (queueItems is { Length: > 0 })
            {
                await LocalisationService
                    .ReplyMessageAsync(message, "PlaylistQueued", message.Author.Mention, playlistInformation.Title)
                    .DeleteMessageAfter(TimeSpan.FromSeconds(5));
            }
            else
            {
                await LocalisationService
                    .ReplyMessageAsync(message, "SongNotFound", message.Author.Mention, string.Empty)
                    .DeleteMessageAfter(TimeSpan.FromSeconds(5));
            }
        }
    }
}