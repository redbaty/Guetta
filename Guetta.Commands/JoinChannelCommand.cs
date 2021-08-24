using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Guetta.Abstractions;
using Guetta.App;

namespace Guetta.Commands
{
    internal class JoinChannelCommand : IDiscordCommand
    {
        public JoinChannelCommand(AudioChannelService audioChannelService)
        {
            AudioChannelService = audioChannelService;
        }
        
        private AudioChannelService AudioChannelService { get; }

        public async Task ExecuteAsync(SocketMessage message, string[] args)
        {
            if (message.Author is IGuildUser {VoiceChannel: { }} author)
            {
                await AudioChannelService.Join(author.VoiceChannel);
            }
            else
            {
                await message.Channel.SendMessageAsync("AUF2");
            }
        }
    }
}