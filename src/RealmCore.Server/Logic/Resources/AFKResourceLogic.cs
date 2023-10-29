namespace RealmCore.Server.Logic.Resources;

internal sealed class AFKResourceLogic
{
    private readonly IEntityEngine _entityEngine;
    private readonly ILogger<StatisticsCounterResourceLogic> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AFKResourceLogic(IAFKService afkService, IEntityEngine entityEngine, ILogger<StatisticsCounterResourceLogic> logger, IDateTimeProvider dateTimeProvider)
    {
        _entityEngine = entityEngine;
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
        afkService.PlayerAFKStarted += HandlePlayerAFKStarted;
        afkService.PlayerAFKStopped += HandlePlayerAFKStopped;
    }

    private void HandlePlayerAFKStarted(Player player)
    {
        if (_entityEngine.TryGetEntityByPlayer(player, out var entity) && entity != null)
        {
            if (entity.TryGetComponent(out AFKComponent afkComponent))
            {
                using var _ = _logger.BeginEntity(entity);
                _logger.LogInformation("Player started AFK");
                afkComponent.HandlePlayerAFKStarted(_dateTimeProvider.Now);
            }
        }
    }

    private void HandlePlayerAFKStopped(Player player)
    {
        if (_entityEngine.TryGetEntityByPlayer(player, out var entity) && entity != null)
        {
            if (entity.TryGetComponent(out AFKComponent afkComponent))
            {
                using var _ = _logger.BeginEntity(entity);
                _logger.LogInformation("Player stopped AFK");
                afkComponent.HandlePlayerAFKStopped(_dateTimeProvider.Now);
            }
        }
    }
}
