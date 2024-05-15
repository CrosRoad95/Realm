namespace RealmCore.Server.Modules.Players.AFK;

internal sealed class AFKResourceHostedService : IHostedService
{
    private readonly IAFKService _afkService;
    private readonly ILogger<StatisticsCounterResourceHostedService> _logger;

    public AFKResourceHostedService(IAFKService afkService, ILogger<StatisticsCounterResourceHostedService> logger)
    {
        _afkService = afkService;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _afkService.PlayerAFKStarted += HandlePlayerAFKStarted;
        _afkService.PlayerAFKStopped += HandlePlayerAFKStopped;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _afkService.PlayerAFKStarted -= HandlePlayerAFKStarted;
        _afkService.PlayerAFKStopped -= HandlePlayerAFKStopped;
        return Task.CompletedTask;
    }

    private void HandlePlayerAFKStarted(Player plr)
    {
        var player = (RealmPlayer)plr;
        using var _ = _logger.BeginElement(player);
        player.AFK.HandleAFKStarted();
    }

    private void HandlePlayerAFKStopped(Player plr)
    {
        var player = (RealmPlayer)plr;
        using var _ = _logger.BeginElement(player);
        player.AFK.HandleAFKStopped();
    }
}
