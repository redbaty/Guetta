using Guetta.App;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Guetta.Api.HealthChecks;

public class DiscordCheck : IHealthCheck
{
    public DiscordCheck(SocketClientEventsService socketClientEventsService)
    {
        SocketClientEventsService = socketClientEventsService;
    }

    private SocketClientEventsService SocketClientEventsService { get; }
    
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        if (SocketClientEventsService.Zombied)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy());
        }
        
        return Task.FromResult(HealthCheckResult.Healthy());
    }
}