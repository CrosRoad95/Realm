namespace Realm.Server.Logic.Resources;

internal class StatisticsCounterLogic
{
    private readonly StatisticsCounterService _statisticsCounterService;
    private readonly MtaServer _mtaServer;
    private readonly ECS _ecs;
    private readonly ILogger<StatisticsCounterLogic> _logger;

    public StatisticsCounterLogic(StatisticsCounterService statisticsCounterService, MtaServer mtaServer, ECS ecs, ILogger<StatisticsCounterLogic> logger)
    {
        _statisticsCounterService = statisticsCounterService;
        _mtaServer = mtaServer;
        _ecs = ecs;
        _logger = logger;
        _ecs.EntityCreated += HandleEntityCreated;
        statisticsCounterService.StatisticsCollected += HandleStatisticsCollected;
        //StatisticsCounterService.SetCounterEnabledFor(player, true);
    }

    private void HandleStatisticsCollected(Player player, Dictionary<int, float> statistics)
    {
        try
        {
            var entity = _ecs.GetEntityByPlayer(player);
            var statisticsCounterComponent = entity.GetRequiredComponent<StatisticsCounterComponent>();
            foreach (var item in statistics)
            {
                try
                {
                    statisticsCounterComponent.IncreaseStat(item.Key, item.Value);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Failed to increase stat for player {playerName} serial: {serial}", player.Name, player.Client.Serial);
                }
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to collect logs for player {playerName} serial: {serial}", player.Name, player.Client.Serial);
        }
    }

    private void HandleEntityCreated(Entity entity)
    {
        if (entity.Tag != Entity.EntityTag.Player)
            return;

        entity.ComponentAdded += HandleComponentAdded;
        entity.ComponentDetached += HandleComponentDetached;
    }

    private void HandleComponentAdded(Component component)
    {
        if (component is not StatisticsCounterComponent) return;

        var playerElementComponent = component.Entity.GetRequiredComponent<PlayerElementComponent>();
        _statisticsCounterService.SetCounterEnabledFor(playerElementComponent.Player, true);
    }

    private void HandleComponentDetached(Component component)
    {
        if(component is not StatisticsCounterComponent) return;

        var playerElementComponent = component.Entity.GetRequiredComponent<PlayerElementComponent>();
        _statisticsCounterService.SetCounterEnabledFor(playerElementComponent.Player, false);
    }
}
