using System;
using System.Linq;
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
        public PlayChannelCommand(QueueService queueService, AudioChannelService audioChannelService,
            YoutubeDlService youtubeDlService, LocalisationService localisationService)
        {
            QueueService = queueService;
            AudioChannelService = audioChannelService;
            YoutubeDlService = youtubeDlService;
            LocalisationService = localisationService;
        }

        private QueueService QueueService { get; }

        private AudioChannelService AudioChannelService { get; }

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

            if (!QueueService.CanPlay())
            {
                if (message.Author is DiscordMember { VoiceState: { Channel: { } } } discordMember)
                {
                    await AudioChannelService.Join(discordMember.VoiceState.Channel);
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
            string input;


            if (Uri.TryCreate(url, UriKind.Absolute, out _))
            {
                input = url;
            }
            else
            {
                input = $"ytsearch:{message.Content}";
            }

            var videoInformation = await YoutubeDlService.GetVideoInformation(input);

            await LocalisationService
                .SendMessageAsync(message.Channel, "SongQueued", message.Author.Mention, videoInformation.Title)
                .DeleteMessageAfter(TimeSpan.FromSeconds(5));

            QueueService.Enqueue(new QueueItem
            {
                User = message.Author,
                Channel = message.Channel,
                YoutubeDlInput = input,
                VideoInformation = videoInformation
            });
        }
    }
}