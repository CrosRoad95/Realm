using SlipeServer.Server.Elements;

namespace Realm.Resources.StatisticsCounter;

internal sealed class StatisticsCounterService : IStatisticsCounterService
{
    public event Action<Player, bool>? CounterStateChanged;
    public event Action<Player, Dictionary<int, float>>? StatisticsCollected;
    public event Action<Player, float, float, float>? FpsStatisticsCollected;

    public void SetCounterEnabledFor(Player player, bool enabled)
    {
        CounterStateChanged?.Invoke(player, enabled);
    }

    public void RelayCollectedStatistics(Player player, Dictionary<int, float> statistics)
    {
        StatisticsCollected?.Invoke(player, statistics);
    }

    public void RelayFpsCollectedStatistics(Player player, float minFps, float maxFps, float avgFps)
    {
        FpsStatisticsCollected?.Invoke(player, minFps, maxFps, avgFps);
    }
}
