using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Guetta.Abstractions;
using Guetta.App;
using Guetta.App.Extensions;
using Guetta.Localisation;

namespace Guetta.Commands;

public class ResetCommand : IDiscordCommand
{
    public ResetCommand(GuildContextManager guildContextManager, LocalisationService localisationService)
    {
        GuildContextManager = guildContextManager;
        LocalisationService = localisationService;
    }

    private GuildContextManager GuildContextManager { get; }
    
    private LocalisationService LocalisationService { get; }
    
    public async Task ExecuteAsync(DiscordMessage message, string[] arguments)
    {
        if (!message.Channel.GuildId.HasValue)
        {
            await message.Channel.SendMessageAsync("Invalid guild ID in channel");
            return;
        }

        var guildContext = GuildContextManager.GetOrCreate(message.Channel.GuildId.Value);

        if (guildContext.GuildQueue.Count <= 0)
        {
            await LocalisationService
                .SendMessageAsync(message.Channel, "NoSongsInQueue")
                .DeleteMessageAfter(TimeSpan.FromSeconds(10));
        }
        else
        {
            guildContext.GuildQueue.Clear();

            await LocalisationService
                .SendMessageAsync(message.Channel, "QueueCleared")
                .DeleteMessageAfter(TimeSpan.FromSeconds(10));
        }
    }
}