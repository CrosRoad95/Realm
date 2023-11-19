namespace RealmCore.Server.Services.Players;

internal class PlayerStatisticsService : IPlayerStatisticsService
{
    private readonly object _lock = new();
    private readonly IPlayerUserService _playerUserService;
    private ICollection<UserStatData> _stats = [];

    public event Action<IPlayerStatisticsService, int, float>? Decreased;
    public event Action<IPlayerStatisticsService, int, float>? Increased;
    public IReadOnlyList<int> StatsIds
    {
        get
        {
            lock(_lock)
                return _stats.Select(x => x.StatId).ToList();
        }
    }

    public RealmPlayer Player { get; private set; }
    public PlayerStatisticsService(PlayerContext playerContext, IPlayerUserService playerUserService)
    {
        Player = playerContext.Player;
        playerUserService.SignedIn += HandleSignedIn;
        playerUserService.SignedOut += HandleSignedOut;
        _playerUserService = playerUserService;
        Player.GetRequiredService<IStatisticsCounterService>().SetCounterEnabledFor(Player, true);
    }

    private void HandleSignedIn(IPlayerUserService playerUserService, RealmPlayer _)
    {
        lock (_lock)
            _stats = playerUserService.User.Stats;
    }

    private void HandleSignedOut(IPlayerUserService playerUserService, RealmPlayer _)
    {
        lock (_lock)
            _stats = [];
    }

    private UserStatData GetStatById(int id)
    {
        var stat = _stats.FirstOrDefault(x => x.StatId == id);
        if(stat == null)
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
            if(stat.Value > old)
                Increased?.Invoke(this, statId, stat.Value);
            else if(stat.Value < old)
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

    public IEnumerator<UserStatDTO> GetEnumerator()
    {
        lock (_lock)
            return new List<UserStatDTO>(_stats.Select(UserStatDTO.Map)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
