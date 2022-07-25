using Guetta.App;

namespace Guetta.Api.HostedServices;

public class YoutubeDlUpdater : BackgroundService
{
    public YoutubeDlUpdater(YoutubeDlService youtubeDlService)
    {
        YoutubeDlService = youtubeDlService;
    }

    private PeriodicTimer PeriodicTimer { get; } = new(TimeSpan.FromDays(1));
    
    private YoutubeDlService YoutubeDlService { get; }
    
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (await PeriodicTimer.WaitForNextTickAsync(cancellationToken) && !cancellationToken.IsCancellationRequested)
        {
            await YoutubeDlService.TryUpdate();
        }
    }
}