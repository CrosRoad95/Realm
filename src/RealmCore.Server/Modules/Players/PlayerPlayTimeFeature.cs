namespace RealmCore.Server.Modules.Players;

public interface IPlayerPlayTimeFeature : IPlayerFeature
{
    TimeSpan PlayTime { get; }
    TimeSpan TotalPlayTime { get; }

    event Action<IPlayerPlayTimeFeature>? MinutePlayed;
    event Action<IPlayerPlayTimeFeature>? MinuteTotalPlayed;

    void InternalSetTotalPlayTime(ulong time);
    void Reset();
    internal void Update();
}

internal sealed class PlayerPlayTimeFeature : IPlayerPlayTimeFeature, IDisposable
{
    private readonly object _lock = new();
    private DateTime _startDateTime;
    private ulong _totalPlayTime = 0;
    private readonly IPlayerUserFeature _playerUserFeature;
    private readonly IDateTimeProvider _dateTimeProvider;
    private int _lastMinute = 0;
    private int _lastMinuteTotal = -1;

    public event Action<IPlayerPlayTimeFeature>? MinutePlayed;
    public event Action<IPlayerPlayTimeFeature>? MinuteTotalPlayed;

    public TimeSpan PlayTime => _dateTimeProvider.Now - _startDateTime;
    public TimeSpan TotalPlayTime => PlayTime + TimeSpan.FromSeconds(_totalPlayTime);

    public RealmPlayer Player { get; init; }

    public PlayerPlayTimeFeature(PlayerContext playerContext, IPlayerUserFeature playerUserFeature, IDateTimeProvider dateTimeProvider)
    {
        Player = playerContext.Player;
        _playerUserFeature = playerUserFeature;
        _dateTimeProvider = dateTimeProvider;
        _startDateTime = _dateTimeProvider.Now;
        _playerUserFeature.SignedIn += HandleSignedIn;
        _playerUserFeature.SignedOut += HandleSignedOut;
    }

    private void HandleSignedIn(IPlayerUserFeature playerUserFeature, RealmPlayer _)
    {
        lock (_lock)
            _totalPlayTime = playerUserFeature.UserData.PlayTime;
    }

    private void HandleSignedOut(IPlayerUserFeature playerUserFeature, RealmPlayer _)
    {
        lock (_lock)
            _totalPlayTime = 0;
    }

    public void InternalSetTotalPlayTime(ulong time)
    {
        _totalPlayTime = time;
    }

    public void Reset()
    {
        _startDateTime = _dateTimeProvider.Now;
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
                _playerUserFeature.IncreaseVersion();
            }
        }
    }
    public void Dispose()
    {
        _playerUserFeature.SignedIn -= HandleSignedIn;
        _playerUserFeature.SignedOut -= HandleSignedOut;
    }
}
