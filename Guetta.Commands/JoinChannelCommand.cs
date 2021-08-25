using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
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

        public async Task ExecuteAsync(DiscordMessage message, string[] args)
        {
            if (message.Author is DiscordMember { VoiceState: {Channel: {}}} discordMember)
            {
                await AudioChannelService.Join(discordMember.VoiceState.Channel);
            }
            else
            {
                await message.Channel.SendMessageAsync("AUF2");
            }
        }
    }
}