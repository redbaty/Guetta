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
        public PlayChannelCommand(LocalisationService localisationService, ILogger<PlayChannelCommand> logger, QueueProxyService queueProxyService, YoutubeDlService youtubeDlService)
        {
            LocalisationService = localisationService;
            Logger = logger;
            QueueProxyService = queueProxyService;
            YoutubeDlService = youtubeDlService;
        }

        private LocalisationService LocalisationService { get; }

        private ILogger<PlayChannelCommand> Logger { get; }
        
        private QueueProxyService QueueProxyService { get; }
        
        private YoutubeDlService YoutubeDlService { get; }

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
            var results = 0;
            
            await foreach (var information in YoutubeDlService.GetVideoInformation(searchTerm, CancellationToken.None))
            {
                Logger.LogInformation("Source information gathered: {@Information}", information);
                QueueProxyService.Enqueue(discordMember.VoiceState.Channel.Id, message.ChannelId, message.Author.Mention, information);
                results++;
            }
            
            Logger.LogInformation("Total information gathered: {@Results}", results);

            switch (results)
            {
                case 0:
                    await message.Channel.SendMessageAsync("No results from that search query")
                        .DeleteMessageAfter(TimeSpan.FromSeconds(5));
                    break;
                case 1:
                    await LocalisationService
                        .SendMessageAsync(message.Channel, "SongQueued", message.Author.Mention, string.Empty)
                        .DeleteMessageAfter(TimeSpan.FromSeconds(5));
                    break;
                default:
                    await LocalisationService
                        .SendMessageAsync(message.Channel, "PlaylistQueued", message.Author.Mention, string.Empty)
                        .DeleteMessageAfter(TimeSpan.FromSeconds(5));
                    break;
            }
        }
    }
}