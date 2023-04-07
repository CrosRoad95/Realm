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
