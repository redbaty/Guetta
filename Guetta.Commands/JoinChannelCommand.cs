using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using Guetta.Abstractions;
using Guetta.App;

namespace Guetta.Commands
{
    internal class JoinChannelCommand : IDiscordCommand
    {
        public JoinChannelCommand(GuildContextManager guildContextManager)
        {
            GuildContextManager = guildContextManager;
        }

        private GuildContextManager GuildContextManager { get; }

        public async Task ExecuteAsync(DiscordMessage message, string[] args)
        {
            if (message.Author is DiscordMember { VoiceState.Channel.GuildId: { } } discordMember)
            {
                var guildContext = GuildContextManager.GetOrCreate(discordMember.VoiceState.Channel.GuildId.Value);
                await guildContext.Voice.Join(discordMember.VoiceState.Channel);
            }
            else
            {
                await message.Channel.SendMessageAsync("AUF2");
            }
        }
    }
}