using Guetta.App;
using Microsoft.AspNetCore.Mvc;

namespace Guetta.Api.Controllers;

[Route("[controller]")]
public class QueueController : ControllerBase
{
    [HttpGet("{contextId}")]
    public IActionResult GetQueueItems([FromRoute] ulong contextId, [FromServices] GuildContextManager guildContextManager)
    {
        var guildContext = guildContextManager.GetOrDefault(contextId);

        if (guildContext == null)
            return NotFound();

        return Ok(guildContext.GuildQueue.GetQueueItems()
            .Select(i => new
            {
                i.VideoInformation.Title,
                i.VideoInformation.Url,
                i.Playing,
                i.CurrentQueueIndex
            })
            .ToArray());
    }
}