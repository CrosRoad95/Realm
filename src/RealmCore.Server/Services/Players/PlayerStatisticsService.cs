using System.Collections;

namespace RealmCore.Server.Services.Players;

internal class PlayerStatisticsService : IPlayerStatisticsService
{
    private readonly object _lock = new();
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
    }

    private void HandleSignedIn(IPlayerUserService playerUserService)
    {
        lock (_lock)
            _stats = playerUserService.User.Stats;
    }

    private void HandleSignedOut(IPlayerUserService playerUserService)
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

    private static UserStatDTO? Map(UserStatData? userStatData)
    {
        if (userStatData == null)
            return null;

        return new UserStatDTO
        {
            StatId = userStatData.StatId,
            Value = userStatData.Value,
        };
    }

    public IEnumerator<UserStatDTO> GetEnumerator()
    {
        lock (_lock)
            return new List<UserStatDTO>(_stats.Select(Map)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }
}
