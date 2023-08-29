using RealmCore.Persistence.Data;

namespace RealmCore.Server.Components.Players;

[ComponentUsage(false)]
public sealed class StatisticsCounterComponent : Component
{
    private readonly ConcurrentDictionary<int, float> _stats = new ConcurrentDictionary<int, float>();

    public IEnumerable<int> GetStatsIds => _stats.Keys;

    public event Action<StatisticsCounterComponent, int, float>? StatIncreased;

    public StatisticsCounterComponent()
    {
    }

    internal StatisticsCounterComponent(IEnumerable<UserStatData> statistics)
    {
        foreach (var item in statistics)
        {
            _stats.TryAdd(item.StatId, item.Value);
        }
    }

    public void IncreaseStat(int statId, float value)
    {
        ThrowIfDisposed();

        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));

        if (_stats.TryGetValue(statId, out var stat))
        {
            if (_stats.TryUpdate(statId, stat + value, stat))
                StatIncreased?.Invoke(this, statId, value);
        }
        else
        {
            if (_stats.TryAdd(statId, value))
                StatIncreased?.Invoke(this, statId, value);
        }
    }

    public float GetStat(int statId)
    {
        ThrowIfDisposed();

        _stats.TryGetValue(statId, out var statValue);
        return statValue;
    }
}
