using SlipeServer.Server.Elements;

namespace RealmCore.Resources.StatisticsCounter;

public interface IStatisticsCounterService
{
    event Action<Player, Dictionary<int, float>>? StatisticsCollected;
    event Action<Player, float, float, float>? FpsStatisticsCollected;
    internal event Action<Player, bool>? CounterStateChanged;

    void RelayCollectedStatistics(Player player, Dictionary<int, float> statistics);
    void RelayFpsCollectedStatistics(Player player, float minFps, float maxFps, float avgFps);
    void SetCounterEnabledFor(Player player, bool enabled);
}

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
