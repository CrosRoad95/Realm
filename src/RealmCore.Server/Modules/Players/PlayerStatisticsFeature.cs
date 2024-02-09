namespace RealmCore.Server.Modules.Players;

public interface IPlayerStatisticsFeature : IPlayerFeature, IEnumerable<UserStatDto>
{
    IReadOnlyList<int> StatsIds { get; }

    event Action<IPlayerStatisticsFeature, int, float>? Decreased;
    event Action<IPlayerStatisticsFeature, int, float>? Increased;

    void Increase(int statId, float value = 1);
    void Decrease(int statId, float value = 1);
    void Set(int statId, float value);
    float Get(int statId);
}

internal sealed class PlayerStatisticsFeature : IPlayerStatisticsFeature
{
    private readonly object _lock = new();
    private readonly IPlayerUserFeature _playerUserService;
    private ICollection<UserStatData> _stats = [];

    public event Action<IPlayerStatisticsFeature, int, float>? Decreased;
    public event Action<IPlayerStatisticsFeature, int, float>? Increased;
    public IReadOnlyList<int> StatsIds
    {
        get
        {
            lock (_lock)
                return _stats.Select(x => x.StatId).ToList();
        }
    }

    public RealmPlayer Player { get; init; }
    public PlayerStatisticsFeature(PlayerContext playerContext, IPlayerUserFeature playerUserService)
    {
        Player = playerContext.Player;
        playerUserService.SignedIn += HandleSignedIn;
        playerUserService.SignedOut += HandleSignedOut;
        _playerUserService = playerUserService;
        Player.GetRequiredService<IStatisticsCounterService>().SetCounterEnabledFor(Player, true);
    }

    private void HandleSignedIn(IPlayerUserFeature playerUserService, RealmPlayer _)
    {
        lock (_lock)
            _stats = playerUserService.User.Stats;
    }

    private void HandleSignedOut(IPlayerUserFeature playerUserService, RealmPlayer _)
    {
        lock (_lock)
            _stats = [];
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
            _playerUserService.IncreaseVersion();
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
            _playerUserService.IncreaseVersion();
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
            _playerUserService.IncreaseVersion();
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
            _playerUserService.IncreaseVersion();
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

    public IEnumerator<UserStatDto> GetEnumerator()
    {
        lock (_lock)
            return new List<UserStatDto>(_stats.Select(UserStatDto.Map)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
