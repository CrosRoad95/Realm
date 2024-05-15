namespace RealmCore.Server.Modules.Players;

internal sealed class StatisticsCounterResourceHostedService : IHostedService
{
    private readonly IStatisticsCounterService _statisticsCounterService;
    private readonly ILogger<StatisticsCounterResourceHostedService> _logger;

    public StatisticsCounterResourceHostedService(IStatisticsCounterService statisticsCounterService, ILogger<StatisticsCounterResourceHostedService> logger)
    {
        _statisticsCounterService = statisticsCounterService;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _statisticsCounterService.StatisticsCollected += HandleStatisticsCollected;
        //statisticsCounterService.FpsStatisticsCollected += HandleFpsStatisticsCollected;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _statisticsCounterService.StatisticsCollected += HandleStatisticsCollected;
        //statisticsCounterService.FpsStatisticsCollected += HandleFpsStatisticsCollected;
        return Task.CompletedTask;
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
