using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Guetta.Abstractions;
using Guetta.App;
using Guetta.App.Extensions;
using Guetta.Localisation;

namespace Guetta.Commands
{
    internal class PlayChannelCommand : IDiscordCommand
    {
        public PlayChannelCommand(LocalisationService localisationService, GuildContextManager guildContextManager, DiscordClient discordClient, VideoInformationService videoInformationService)
        {
            LocalisationService = localisationService;
            GuildContextManager = guildContextManager;
            DiscordClient = discordClient;
            VideoInformationService = videoInformationService;
        }

        private GuildContextManager GuildContextManager { get; }

        private VideoInformationService VideoInformationService { get; }

        private LocalisationService LocalisationService { get; }

        private DiscordClient DiscordClient { get; }

        public async Task ExecuteAsync(DiscordMessage message, string[] arguments)
        {
            if (arguments.Length < 1)
            {
                await LocalisationService
                    .ReplyMessageAsync(message, "InvalidArgument", message.Author.Mention)
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

            if (message.Author is not DiscordMember { VoiceState: { Channel: { } } } discordMember)
            {
                await LocalisationService
                    .ReplyMessageAsync(message, "NotInChannel", message.Author.Mention)
                    .DeleteMessageAfter(TimeSpan.FromSeconds(5));
                return;
            }

            await message.Channel.TriggerTypingAsync();
            var url = arguments.Aggregate((x, y) => $"{x} {y}");

            var playlistInformation = await VideoInformationService.GetVideoInformation(url);
            var queueItems = playlistInformation?.Videos
                .Select(i => new QueueItem
                {
                    User = message.Author,
                    TextChannel = message.Channel,
                    VoiceChannel = discordMember.VoiceState.Channel, 
                    VideoInformation = i
                }).ToArray();

            if (queueItems is { Length: > 1 })
            {
                var positiveEmoji = DiscordEmoji.FromName(DiscordClient, ":white_check_mark:");
                var negativeEmoji = DiscordEmoji.FromName(DiscordClient, ":x:");
                var content = string.Format(LocalisationService.GetMessageTemplate("MultipleSongsConfirmation"), queueItems.Length, positiveEmoji, negativeEmoji);
                var confirmPlaylistQueue = await message.AskReply(content, message.Author, LocalisationService.GetMessageTemplate("MultipleSongsConfirmationPositiveButton"), positiveEmoji, LocalisationService.GetMessageTemplate("MultipleSongsConfirmationNegativeButton"), negativeEmoji, TimeSpan.FromSeconds(10));

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