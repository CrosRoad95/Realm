namespace RealmCore.Server.Components.Players;

[ComponentUsage(false)]
public sealed class StatisticsCounterComponent : Component
{
    private readonly ConcurrentDictionary<int, float> _stats = new();

    public IEnumerable<int> GetStatsIds => _stats.Keys;
    public IReadOnlyDictionary<int, float> Statistics => new Dictionary<int, float>(_stats);

    public event Action<StatisticsCounterComponent, int, float>? StatDecreased;
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
    
    internal StatisticsCounterComponent(Dictionary<int, float> defaults)
    {
        _stats = new(defaults);
    }

    public void IncreaseStat(int statId, float value)
    {
        ThrowIfDisposed();

        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));

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

    public void DecreaseStat(int statId, float value)
    {
        ThrowIfDisposed();

        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));

        if (_stats.TryGetValue(statId, out var stat))
        {
            if (_stats.TryUpdate(statId, stat - value, stat))
                StatDecreased?.Invoke(this, statId, value);
        }
        else
        {
            if (_stats.TryAdd(statId, value))
                StatDecreased?.Invoke(this, statId, value);
        }
    }

    public void SetStat(int statId, float value)
    {
        ThrowIfDisposed();

        if (_stats.TryGetValue(statId, out var previousStat))
        {
            if (_stats.TryUpdate(statId, value, previousStat))
            {
                if(value > previousStat)
                    StatIncreased?.Invoke(this, statId, value);
                else if(value < previousStat)
                    StatDecreased?.Invoke(this, statId, value);
            }
        }
        else
        {
            if (_stats.TryAdd(statId, value))
            {
                if(value > 0)
                {
                    StatIncreased?.Invoke(this, statId, value);
                }
                else
                {
                    StatDecreased?.Invoke(this, statId, value);
                }
            }
        }
    }

    public float GetStat(int statId)
    {
        ThrowIfDisposed();

        _stats.TryGetValue(statId, out var statValue);
        return statValue;
    }
}
