namespace RealmCore.Server.Logic.Resources;

internal sealed class StatisticsCounterResourceLogic : ComponentLogic<StatisticsCounterComponent>
{
    private readonly IStatisticsCounterService _statisticsCounterService;
    private readonly ILogger<StatisticsCounterResourceLogic> _logger;

    public StatisticsCounterResourceLogic(IStatisticsCounterService statisticsCounterService, IElementFactory elementFactory, ILogger<StatisticsCounterResourceLogic> logger) : base(elementFactory)
    {
        _statisticsCounterService = statisticsCounterService;
        _logger = logger;
        statisticsCounterService.StatisticsCollected += HandleStatisticsCollected;
        //statisticsCounterService.FpsStatisticsCollected += HandleFpsStatisticsCollected;
    }

    private void HandleFpsStatisticsCollected(Player plr, float minFps, float maxFps, float avgFps)
    {
        // TODO:
    }

    private void HandleStatisticsCollected(Player player, Dictionary<int, float> statistics)
    {
        try
        {
            var statisticsCounterComponent = ((RealmPlayer)player).Components.GetRequiredComponent<StatisticsCounterComponent>();
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
                    _logger.LogError(ex, "Failed to increase stat for player {playerName} serial: {serial}", player.Name, player.Client.TryGetSerial());
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to collect logs for player {playerName} serial: {serial}", player.Name, player.Client.TryGetSerial());
        }
    }

    protected override void ComponentAdded(StatisticsCounterComponent component)
    {
        var player = (RealmPlayer)component.Element;
        _statisticsCounterService.SetCounterEnabledFor(player, true);
    }

    protected override void ComponentDetached(StatisticsCounterComponent component)
    {
        var player = (RealmPlayer)component.Element;
        _statisticsCounterService.SetCounterEnabledFor(player, false);
    }
}
