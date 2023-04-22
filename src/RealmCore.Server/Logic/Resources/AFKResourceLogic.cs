namespace RealmCore.Server.Logic.Resources;

internal sealed class AFKResourceLogic
{
    private readonly IECS _ecs;
    private readonly ILogger<StatisticsCounterResourceLogic> _logger;

    public AFKResourceLogic(IAFKService afkService,
        IECS ecs, ILogger<StatisticsCounterResourceLogic> logger)
    {
        _ecs = ecs;
        _logger = logger;
        afkService.PlayerAFKStarted += HandlePlayerAFKStarted;
        afkService.PlayerAFKStopped += HandlePlayerAFKStopped;
    }

    private void HandlePlayerAFKStarted(Player player)
    {
        if (_ecs.TryGetEntityByPlayer(player, out var entity))
        {
            if (entity.TryGetComponent(out AFKComponent afkComponent))
            {
                afkComponent.HandlePlayerAFKStarted();
            }
        }
    }

    private void HandlePlayerAFKStopped(Player player)
    {
        if (_ecs.TryGetEntityByPlayer(player, out var entity))
        {
            if (entity.TryGetComponent(out AFKComponent afkComponent))
            {
                afkComponent.HandlePlayerAFKStopped();
            }
        }
    }
}
