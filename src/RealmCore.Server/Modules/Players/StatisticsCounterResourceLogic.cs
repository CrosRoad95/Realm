namespace RealmCore.Server.Modules.Players;

internal sealed class StatisticsCounterResourceLogic
{
    private readonly ILogger<StatisticsCounterResourceLogic> _logger;

    public StatisticsCounterResourceLogic(IStatisticsCounterService statisticsCounterService, ILogger<StatisticsCounterResourceLogic> logger)
    {
        _logger = logger;
        statisticsCounterService.StatisticsCollected += HandleStatisticsCollected;
        //statisticsCounterService.FpsStatisticsCollected += HandleFpsStatisticsCollected;
    }

    private void HandleFpsStatisticsCollected(Player plr, float minFps, float maxFps, float avgFps)
    {
        // TODO:
    }

    private void HandleStatisticsCollected(Player plr, Dictionary<int, float> statistics)
    {
        var player = (RealmPlayer)plr;
        try
        {
            foreach (var item in statistics)
            {
                try
                {
                    if (item.Key < 0 || item.Key > 1000)
                        throw new Exception($"Received out of range statId from client: {item.Key}");
                    player.Statistics.Increase(item.Key, item.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to increase stat for player {playerName} serial: {serial}", player.Name, player.Client.GetSerialOrDefault());
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to collect logs for player {playerName} serial: {serial}", player.Name, player.Client.GetSerialOrDefault());
        }
    }
}
