namespace RealmCore.Server.Components.Players;

[ComponentUsage(false)]
public class PlayTimeComponent : Component, IRareUpdateCallback
{
    private DateTime _startDateTime;
    private readonly ulong _totalPlayTime = 0;
    private readonly IDateTimeProvider _dateTimeProvider;
    private int _lastMinute = 0;
    private int _lastMinuteTotal = -1;

    public event Action<PlayTimeComponent>? MinutePlayed;
    public event Action<PlayTimeComponent>? MinuteTotalPlayed;

    public TimeSpan PlayTime => _dateTimeProvider.Now - _startDateTime;
    public TimeSpan TotalPlayTime => PlayTime + TimeSpan.FromSeconds(_totalPlayTime);

    public PlayTimeComponent(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
        _startDateTime = _dateTimeProvider.Now;
    }

    public PlayTimeComponent(IDateTimeProvider dateTimeProvider, ulong totalPlayTime)
    {
        _dateTimeProvider = dateTimeProvider;
        _totalPlayTime = totalPlayTime;
        _lastMinuteTotal = (int)TotalPlayTime.TotalMinutes;
        _startDateTime = _dateTimeProvider.Now;
    }

    public void Reset()
    {
        _startDateTime = _dateTimeProvider.Now;
    }

    public void RareUpdate()
    {
        if((int)PlayTime.TotalMinutes != _lastMinute)
        {
            _lastMinute = (int)PlayTime.TotalMinutes;
            MinutePlayed?.Invoke(this);
        }

        if((int)TotalPlayTime.TotalMinutes != _lastMinuteTotal)
        {
            _lastMinuteTotal = (int)TotalPlayTime.TotalMinutes;
            MinuteTotalPlayed?.Invoke(this);
        }
    }
}
