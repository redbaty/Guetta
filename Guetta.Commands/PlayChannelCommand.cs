using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Guetta.Abstractions;
using Guetta.App;
using Guetta.App.Extensions;
using Guetta.Localisation;

namespace Guetta.Commands
{
    internal class PlayChannelCommand : IDiscordCommand
    {
        public PlayChannelCommand(YoutubeDlService youtubeDlService, LocalisationService localisationService, GuildContextManager guildContextManager)
        {
            YoutubeDlService = youtubeDlService;
            LocalisationService = localisationService;
            GuildContextManager = guildContextManager;
        }

        private GuildContextManager GuildContextManager { get; }

        private YoutubeDlService YoutubeDlService { get; }

        private LocalisationService LocalisationService { get; }

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
            var url = arguments.Last();
            var videoFound = false;
            
            
            await foreach (var videoInformation in YoutubeDlService.GetVideoInformation(url, CancellationToken.None))
            {
                videoFound = true;
                
                await LocalisationService
                    .SendMessageAsync(message.Channel, "SongQueued", message.Author.Mention, videoInformation.Title)
                    .DeleteMessageAfter(TimeSpan.FromSeconds(5));
                
                queue.Enqueue(new QueueItem
                {
                    User = message.Author,
                    Channel = message.Channel,
                    VideoInformation = videoInformation
                });
            }

            if (!videoFound)
            {
                
            }
        }
    }
}