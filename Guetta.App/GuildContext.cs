namespace Guetta.App;

public class GuildContext
{
    public ulong Id { get; }

    public GuildQueue GuildQueue { get; }

    public Voice Voice { get; }

    public GuildContext(ulong id, GuildQueue guildQueue, Voice voice)
    {
        Id = id;
        GuildQueue = guildQueue;
        Voice = voice;
    }
}