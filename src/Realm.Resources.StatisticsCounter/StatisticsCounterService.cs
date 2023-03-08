using SlipeServer.Server.Elements;

namespace Realm.Resources.StatisticsCounter;

public class StatisticsCounterService
{
    internal event Action<Player, bool>? CounterStateChanged;
    public event Action<Player, Dictionary<int, float>>? StatisticsCollected;

    public void SetCounterEnabledFor(Player player, bool enabled)
    {
        CounterStateChanged?.Invoke(player, enabled);
    }

    public void RelayCollectedStatistics(Player player, Dictionary<int, float> statistics)
    {
        StatisticsCollected?.Invoke(player, statistics);
    }
}
