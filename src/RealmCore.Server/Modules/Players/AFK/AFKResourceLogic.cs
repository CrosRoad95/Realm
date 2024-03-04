namespace RealmCore.Server.Modules.Players.AFK;

internal sealed class AFKResourceLogic
{
    private readonly ILogger<StatisticsCounterResourceLogic> _logger;

    public AFKResourceLogic(IAFKService afkService, ILogger<StatisticsCounterResourceLogic> logger)
    {
        _logger = logger;
        afkService.PlayerAFKStarted += HandlePlayerAFKStarted;
        afkService.PlayerAFKStopped += HandlePlayerAFKStopped;
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
