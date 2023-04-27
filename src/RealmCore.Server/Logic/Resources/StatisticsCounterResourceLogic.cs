namespace RealmCore.Server.Logic.Resources;

internal sealed class StatisticsCounterResourceLogic
{
    private readonly IStatisticsCounterService _statisticsCounterService;
    private readonly MtaServer _mtaServer;
    private readonly IECS _ecs;
    private readonly ILogger<StatisticsCounterResourceLogic> _logger;

    public StatisticsCounterResourceLogic(IStatisticsCounterService statisticsCounterService, MtaServer mtaServer, IECS ecs, ILogger<StatisticsCounterResourceLogic> logger)
    {
        _statisticsCounterService = statisticsCounterService;
        _mtaServer = mtaServer;
        _ecs = ecs;
        _logger = logger;
        _ecs.EntityCreated += HandleEntityCreated;
        statisticsCounterService.StatisticsCollected += HandleStatisticsCollected;
        //statisticsCounterService.FpsStatisticsCollected += HandleFpsStatisticsCollected;
    }

    //private void HandleFpsStatisticsCollected(Player player, float minFps, float maxFps, float avgFps)
    //{
    //    if (_entityByElement.TryGetEntityByPlayer(player, out var playerEntity))
    //    {
    //        playerEntity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"Fps: {minFps}, {maxFps}, {avgFps}");
    //    }
    //}

    private void HandleStatisticsCollected(Player player, Dictionary<int, float> statistics)
    {
        try
        {
            if (_ecs.TryGetEntityByPlayer(player, out Entity entity))
            {
                var statisticsCounterComponent = entity.GetRequiredComponent<StatisticsCounterComponent>();
                foreach (var item in statistics)
                {
                    try
                    {
                        if (item.Key < 0 || item.Key > 1000)
                            throw new Exception($"Received out of range statId from client: {item.Key}");
                        statisticsCounterComponent.IncreaseStat(item.Key, item.Value);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to increase stat for player {playerName} serial: {serial}", player.Name, player.Client.Serial);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to collect logs for player {playerName} serial: {serial}", player.Name, player.Client.Serial);
        }
    }

    private void HandleEntityCreated(Entity entity)
    {
        if (entity.Tag != EntityTag.Player)
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
        if (component is not StatisticsCounterComponent) return;

        var playerElementComponent = component.Entity.GetRequiredComponent<PlayerElementComponent>();
        _statisticsCounterService.SetCounterEnabledFor(playerElementComponent.Player, false);
    }
}
