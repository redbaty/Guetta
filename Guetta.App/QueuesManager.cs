using System;
using System.Collections.Generic;
using Guetta.Localisation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Guetta.App;

public class GuildContextManager
{
    public GuildContextManager(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    private Dictionary<ulong, GuildContext> ContextByGuild { get; } = new();

    private IServiceProvider ServiceProvider { get; }

    private Voice BuildVoice(ulong guildId) => new(
        ServiceProvider.GetService<YoutubeDlService>(),
        ServiceProvider.GetService<LocalisationService>(),
        guildId
    );

    private GuildQueue BuildQueue(ulong guildId, Voice voice) => new(
        ServiceProvider.GetService<ILogger<GuildQueue>>(),
        ServiceProvider.GetService<LocalisationService>(),
        voice,
        guildId
    );

    private GuildContext BuildContext(ulong guildId)
    {
        var voice = BuildVoice(guildId);
        var queue = BuildQueue(guildId, voice);
        return new GuildContext(guildId, queue, voice);
    }

    public GuildContext GetOrCreate(ulong guildId)
    {
        if (!ContextByGuild.ContainsKey(guildId))
        {
            ContextByGuild.Add(guildId, BuildContext(guildId));
        }

        return ContextByGuild[guildId];
    }
}

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