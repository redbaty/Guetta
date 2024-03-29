﻿using DSharpPlus;
using Guetta.Api.Models;
using Guetta.App;
using Microsoft.AspNetCore.Mvc;

namespace Guetta.Api.Controllers;

[Route("[controller]")]
public class GuildContextController : ControllerBase
{
    [HttpGet]
    public async IAsyncEnumerable<GuildItem> GetContexts([FromServices] GuildContextManager guildContextManager, [FromServices] DiscordClient discordClient)
    {
        foreach (var guildId in guildContextManager.GetActiveGuilds())
        {
            var discordGuild = await discordClient.GetGuildAsync(guildId);

            yield return new GuildItem
            {
                Id = discordGuild.Id.ToString(),
                Name = discordGuild.Name
            };
        }
    }
}