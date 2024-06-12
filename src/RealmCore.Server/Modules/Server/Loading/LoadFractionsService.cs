namespace RealmCore.Server.Modules.Server.Loading;

internal sealed class LoadFractionsService : IHostedService
{
    private readonly IFractionsService _fractionService;

    public LoadFractionsService(IFractionsService fractionService)
    {
        _fractionService = fractionService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _fractionService.LoadFractions(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
