namespace RealmCore.Server.Modules.Players.PlayTime;

public interface IPlayerPlayTimeFeature : IPlayerFeature, IEnumerable<PlayerPlayTimeDto>
{
    TimeSpan PlayTime { get; }
    TimeSpan TotalPlayTime { get; }
    int? Category { get; set; }

    event Action<IPlayerPlayTimeFeature>? MinutePlayed;
    event Action<IPlayerPlayTimeFeature>? MinuteTotalPlayed;
    event Action<IPlayerPlayTimeFeature, int?, int?>? CategoryChanged;

    void InternalSetTotalPlayTime(ulong time);
    void Reset();
    internal void UpdateCategoryPlayTime(int? category, DateTime now);
    internal void Update();
}

internal sealed class PlayerPlayTimeFeature : IPlayerPlayTimeFeature, IUsesUserPersistentData
{
    private readonly object _lock = new();
    private DateTime _startDateTime;
    private ulong _totalPlayTime = 0;
    private readonly IDateTimeProvider _dateTimeProvider;
    private int _lastMinute = 0;
    private int _lastMinuteTotal = -1;
    private DateTime? _lastCategoryPlayTime;

    public event Action<IPlayerPlayTimeFeature>? MinutePlayed;
    public event Action<IPlayerPlayTimeFeature>? MinuteTotalPlayed;
    public event Action? VersionIncreased;

    public TimeSpan PlayTime => _dateTimeProvider.Now - _startDateTime;
    public TimeSpan TotalPlayTime => PlayTime + TimeSpan.FromSeconds(_totalPlayTime);
    private ICollection<UserPlayTimeData> _playTimes = [];
    private UserData? _userData;
    private int? _category;

    public event Action<IPlayerPlayTimeFeature, int?, int?>? CategoryChanged;
    public int? Category
    {
        get => _category; set
        {
            lock (_lock)
            {
                if (_category != value)
                {
                    var previous = _category;
                    _category = value;
                    CategoryChanged?.Invoke(this, previous, value);
                }
            }
        }
    }

    public RealmPlayer Player { get; init; }

    public PlayerPlayTimeFeature(PlayerContext playerContext, IDateTimeProvider dateTimeProvider)
    {
        Player = playerContext.Player;
        _dateTimeProvider = dateTimeProvider;
        _startDateTime = _dateTimeProvider.Now;
    }


    public void SignIn(UserData userData)
    {
        _userData = userData;
        _playTimes = userData.PlayTimes;
    }

    public void SignOut()
    {
        _totalPlayTime = 0;
    }

    public void InternalSetTotalPlayTime(ulong time)
    {
        _totalPlayTime = time;
    }

    public void Reset()
    {
        lock (_lock)
        {
            _startDateTime = _dateTimeProvider.Now;
            _playTimes = [];
        }
    }

    public void UpdateCategoryPlayTime(int? category, DateTime now)
    {
        lock (_lock)
        {
            if (_lastCategoryPlayTime != null)
            {
                if (category != null)
                {
                    var playTime = now - _lastCategoryPlayTime;
                    var userPlayTime = _playTimes.Where(x => x.Category == category).FirstOrDefault();
                    if (userPlayTime != null)
                    {
                        userPlayTime.PlayTime += (int)playTime.Value.TotalSeconds;
                    }
                    else
                    {
                        _playTimes.Add(new UserPlayTimeData
                        {
                            Category = category.Value,
                            PlayTime = (int)playTime.Value.TotalSeconds,
                        });
                    }
                }
            }
            _lastCategoryPlayTime = now;
        }
    }

    public void Update()
    {
        lock (_lock)
        {
            if ((int)PlayTime.TotalMinutes != _lastMinute)
            {
                _lastMinute = (int)PlayTime.TotalMinutes;
                MinutePlayed?.Invoke(this);
            }

            if ((int)TotalPlayTime.TotalMinutes != _lastMinuteTotal)
            {
                _lastMinuteTotal = (int)TotalPlayTime.TotalMinutes;
                MinuteTotalPlayed?.Invoke(this);
                VersionIncreased?.Invoke();
            }
        }
    }

    public IEnumerator<PlayerPlayTimeDto> GetEnumerator()
    {
        lock (_lock)
            return new List<PlayerPlayTimeDto>(_playTimes.Select(PlayerPlayTimeDto.Map)).AsReadOnly().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
