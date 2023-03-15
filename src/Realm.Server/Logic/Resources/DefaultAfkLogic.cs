namespace Realm.Server.Logic.Resources;

internal class DefaultAfkLogic
{
    private readonly IEntityByElement _entityByElement;
    private readonly ILogger<StatisticsCounterLogic> _logger;

    public DefaultAfkLogic(AFKService afkService,
        IEntityByElement entityByElement, ILogger<StatisticsCounterLogic> logger)
    {
        _entityByElement = entityByElement;
        _logger = logger;
        afkService.PlayerAFKStarted += HandlePlayerAFKStarted;
        afkService.PlayerAFKStopped += HandlePlayerAFKStopped;
    }

    private void HandlePlayerAFKStarted(Player player)
    {
        if (_entityByElement.TryGetEntityByPlayer(player, out var entity))
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
        if(_entityByElement.TryGetEntityByPlayer(player, out var entity))
        {
            if(entity.TryGetComponent(out AFKComponent afkComponent))
            {
                _logger.LogInformation("Stopped afk {entity}", entity);
                afkComponent.HandlePlayerAFKStopped();
            }
        }
    }
}
