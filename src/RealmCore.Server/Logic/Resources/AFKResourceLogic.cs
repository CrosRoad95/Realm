namespace RealmCore.Server.Logic.Resources;

internal sealed class AFKResourceLogic
{
    private readonly IEntityEngine _ecs;
    private readonly ILogger<StatisticsCounterResourceLogic> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AFKResourceLogic(IAFKService afkService, IEntityEngine ecs, ILogger<StatisticsCounterResourceLogic> logger, IDateTimeProvider dateTimeProvider)
    {
        _ecs = ecs;
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
        afkService.PlayerAFKStarted += HandlePlayerAFKStarted;
        afkService.PlayerAFKStopped += HandlePlayerAFKStopped;
    }

    private void HandlePlayerAFKStarted(Player player)
    {
        if (_ecs.TryGetEntityByPlayer(player, out var entity))
        {
            if (entity.TryGetComponent(out AFKComponent afkComponent))
            {
                afkComponent.HandlePlayerAFKStarted(_dateTimeProvider.Now);
            }
        }
    }

    private void HandlePlayerAFKStopped(Player player)
    {
        if (_ecs.TryGetEntityByPlayer(player, out var entity))
        {
            if (entity.TryGetComponent(out AFKComponent afkComponent))
            {
                afkComponent.HandlePlayerAFKStopped(_dateTimeProvider.Now);
            }
        }
    }
}
