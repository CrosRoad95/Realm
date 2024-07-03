namespace RealmCore.Server.Modules.Players;

public interface IPlayerStatisticsFeature : IPlayerFeature, IEnumerable<UserStatDto>
{
    int[] StatsIds { get; }
    int[] GtaSaStatsIds { get; }

    event Action<IPlayerStatisticsFeature, int, float>? Decreased;
    event Action<IPlayerStatisticsFeature, int, float>? Increased;

    void Increase(int statId, float value = 1);
    void Decrease(int statId, float value = 1);
    void Set(int statId, float value);
    float Get(int statId);
    void SetGtaSa(PedStat statId, float value);
    float GetGtaSa(PedStat statId);
}

internal sealed class PlayerStatisticsFeature : IPlayerStatisticsFeature, IUsesUserPersistentData, IDisposable
{
    private readonly object _lock = new();
    private ICollection<UserStatData> _stats = [];
    private ICollection<UserGtaStatData> _gtaSaStats = [];

    public event Action<IPlayerStatisticsFeature, int, float>? Decreased;
    public event Action<IPlayerStatisticsFeature, int, float>? Increased;
    public event Action? VersionIncreased;

    public int[] StatsIds
    {
        get
        {
            lock (_lock)
                return _stats.Select(x => x.StatId).ToArray();
        }
    }

    public int[] GtaSaStatsIds
    {
        get
        {
            lock (_lock)
                return _gtaSaStats.Select(x => x.StatId).ToArray();
        }
    }

    public RealmPlayer Player { get; init; }

    private readonly IStatisticsCounterService _statisticsCounterService;
    public PlayerStatisticsFeature(PlayerContext playerContext, IStatisticsCounterService statisticsCounterService)
    {
        Player = playerContext.Player;
        _statisticsCounterService = statisticsCounterService;
    }

    public void LogIn(UserData userData)
    {
        lock (_lock)
        {
            _stats = userData.Stats;
            _gtaSaStats = userData.GtaSaStats;

            foreach (var gtaSaStat in _gtaSaStats)
            {
                Player.SetStat((PedStat)gtaSaStat.StatId, gtaSaStat.Value);
            }

            _statisticsCounterService.SetCounterEnabledFor(Player, true);
        }
    }

    public void LogOut()
    {
        _stats = [];

        foreach (var gtaSaStat in _gtaSaStats)
        {
            Player.SetStat((PedStat)gtaSaStat.StatId, 0);
        }

        _gtaSaStats = [];
    }

    private UserStatData GetStatById(int id)
    {
        var stat = _stats.FirstOrDefault(x => x.StatId == id);
        if (stat == null)
        {
            stat = new UserStatData
            {
                StatId = id,
                Value = 0
            };
            _stats.Add(stat);
            VersionIncreased?.Invoke();
            return stat;
        }
        return stat;
    }
    
    private UserGtaStatData GetGtaSaStatById(PedStat id)
    {
        var stat = _gtaSaStats.FirstOrDefault(x => x.StatId == (int)id);
        if (stat == null)
        {
            stat = new UserGtaStatData
            {
                StatId = (int)id,
                Value = 0
            };
            _gtaSaStats.Add(stat);
            VersionIncreased?.Invoke();
            return stat;
        }
        return stat;
    }

    public void Increase(int statId, float value = 1)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));

        lock (_lock)
        {
            var stat = GetStatById(statId);
            stat.Value += value;
            VersionIncreased?.Invoke();
            Increased?.Invoke(this, statId, stat.Value);
        }
    }

    public void Decrease(int statId, float value = 1)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));

        lock (_lock)
        {
            var stat = GetStatById(statId);
            stat.Value -= value;
            VersionIncreased?.Invoke();
            Increased?.Invoke(this, statId, stat.Value);
        }
    }

    public void Set(int statId, float value)
    {
        lock (_lock)
        {
            var stat = GetStatById(statId);
            var old = stat.Value;
            stat.Value = value;
            if (stat.Value > old)
                Increased?.Invoke(this, statId, stat.Value);
            else if (stat.Value < old)
                Decreased?.Invoke(this, statId, stat.Value);
            VersionIncreased?.Invoke();
        }
    }

    public float Get(int statId)
    {
        lock (_lock)
        {
            var stat = GetStatById(statId);
            return stat.Value;
        }
    }
    
    public void SetGtaSa(PedStat statId, float value)
    {
        lock (_lock)
        {
            var stat = GetGtaSaStatById(statId);
            var old = stat.Value;
            stat.Value = value;
            Player.SetStat(statId, value);
            VersionIncreased?.Invoke();
        }
    }

    public float GetGtaSa(PedStat statId)
    {
        lock (_lock)
        {
            var stat = GetGtaSaStatById(statId);
            return stat.Value;
        }
    }

    public IEnumerator<UserStatDto> GetEnumerator()
    {
        UserStatData[] view;
        lock (_lock)
            view = [.. _stats];

        foreach (var userStatData in view)
        {
            yield return UserStatDto.Map(userStatData);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Dispose()
    {
        _statisticsCounterService.SetCounterEnabledFor(Player, false);
        lock (_lock)
        {
            _stats = [];
            _gtaSaStats = [];
        }
    }
}
