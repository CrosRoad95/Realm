
namespace RealmCore.Server.Modules.Server.Loading;

internal sealed class LoadFractionsService : IHostedLifecycleService
{
    private readonly FractionsService _fractionService;

    public LoadFractionsService(FractionsService fractionService)
    {
        _fractionService = fractionService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task StartingAsync(CancellationToken cancellationToken)
    {
        await _fractionService.LoadFractions(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StoppedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
