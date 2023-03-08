using Realm.Domain.Inventory;
using System.Collections.Concurrent;

namespace Realm.Domain.Components.Players;

[ComponentUsage(false)]
public class StatisticsCounterComponent : Component
{
    [Inject]
    private StatisticsCounterService StatisticsCounterService { get; set; } = default!;

    private readonly ConcurrentDictionary<int, float> _stats = new ConcurrentDictionary<int, float>();

    public IEnumerable<int> GetStatsIds => _stats.Keys;
    public StatisticsCounterComponent()
    {
    }
    
    internal StatisticsCounterComponent(IEnumerable<UserStat> statistics)
    {
        foreach (var item in statistics)
        {
            _stats.TryAdd(item.StatId, item.Value);
        }
    }

    protected override void Load()
    {
        var player = Entity.GetRequiredComponent<PlayerElementComponent>().Player;
    }

    public void IncreaseStat(int statId, float value)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));

        if(_stats.TryGetValue(statId, out var stat))
        {
            _stats.TryUpdate(statId, stat + value, stat);
        }
        else
        {
            _stats.TryAdd(statId, value);
        }
    }

    public float GetStat(int statId)
    {
        _stats.TryGetValue(statId, out var statValue);
        return statValue;
    }
}
