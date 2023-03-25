namespace Realm.Server.Logic.Resources;

internal class DefaultAfkLogic
{
    private readonly IECS _ecs;
    private readonly ILogger<StatisticsCounterLogic> _logger;

    public DefaultAfkLogic(IAFKService afkService,
        IECS ecs, ILogger<StatisticsCounterLogic> logger)
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
                _logger.LogInformation("Started afk {entity}", entity);
                afkComponent.HandlePlayerAFKStarted();
            }
        }
    }

    private void HandlePlayerAFKStopped(Player player)
    {
        if(_ecs.TryGetEntityByPlayer(player, out var entity))
        {
            if(entity.TryGetComponent(out AFKComponent afkComponent))
            {
                _logger.LogInformation("Stopped afk {entity}", entity);
                afkComponent.HandlePlayerAFKStopped();
            }
        }
    }
}
