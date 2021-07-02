using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Guetta.Abstractions;
using Guetta.Extensions;

namespace Guetta.Commands
{
    internal class PlayChannelCommand : IDiscordCommand
    {
        public PlayChannelCommand(QueueService queueService, AudioChannelService audioChannelService, YoutubeDlService youtubeDlService)
        {
            QueueService = queueService;
            AudioChannelService = audioChannelService;
            YoutubeDlService = youtubeDlService;
        }

        private QueueService QueueService { get; }

        private AudioChannelService AudioChannelService { get; }
        
        private YoutubeDlService YoutubeDlService { get; }

        public async Task ExecuteAsync(SocketMessage message, string[] arguments)
        {
            if (arguments.Length < 1)
            {
                await message.Channel.SendMessageAsync("Argumentos inválidos");
                return;
            }

            if (!QueueService.CanPlay())
            {
                if (message.Author is IGuildUser {VoiceChannel: { }} author)
                {
                    await AudioChannelService.Join(author.VoiceChannel);
                }
                else
                {
                    await message.Channel.SendMessageAsync("Não está em canal");
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
            
            QueueService.Enqueue(new QueueItem
            {
                User = message.Author,
                Channel = message.Channel,
                YoutubeDlInput = input,
                VideoInformation = videoInformation
            });

            await message.Channel.SendMessageAsync("Música enfileirada")
                .DeleteMessageAfter(TimeSpan.FromSeconds(5));
        }
    }
}