using Guetta.App;

namespace Guetta.Api.HostedServices;

public class YoutubeDlUpdater : IHostedService
{
    public YoutubeDlUpdater(YoutubeDlService youtubeDlService)
    {
        YoutubeDlService = youtubeDlService;
    }

    private PeriodicTimer PeriodicTimer { get; } = new(TimeSpan.FromDays(1));
    
    private YoutubeDlService YoutubeDlService { get; }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (await PeriodicTimer.WaitForNextTickAsync(cancellationToken) && !cancellationToken.IsCancellationRequested)
        {
            await YoutubeDlService.TryUpdate();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}