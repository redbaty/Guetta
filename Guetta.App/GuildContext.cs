using System.Threading.Channels;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace Guetta.App;

public class GuildContext
{
    public ulong Id { get; }

    public GuildQueue GuildQueue { get; }

    public Voice Voice { get; }

    internal Channel<DiscordMessage> CommandChannel { get; } = Channel.CreateBounded<DiscordMessage>(100);
    
    internal Task CommandChannelTask { get; set; }

    public GuildContext(ulong id, GuildQueue guildQueue, Voice voice)
    {
        Id = id;
        GuildQueue = guildQueue;
        Voice = voice;
    }
}