namespace RealmCore.Server.Components.Players;

[ComponentUsage(false)]
public class PlayTimeComponent : Component
{
    private DateTime? _startDateTime;
    private readonly ulong _currentPlayTime = 0;
    private readonly IDateTimeProvider _dateTimeProvider;

    public TimeSpan PlayTime
    {
        get
        {
            ThrowIfDisposed();

            if (_startDateTime == null)
                return TimeSpan.Zero;
            return _dateTimeProvider.Now - _startDateTime.Value;
        }
    }

    public TimeSpan TotalPlayTime
    {
        get
        {
            ThrowIfDisposed();
            return PlayTime + TimeSpan.FromSeconds(_currentPlayTime);
        }
    }

    public PlayTimeComponent(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public PlayTimeComponent(ulong currentPlayTime, IDateTimeProvider dateTimeProvider)
    {
        _currentPlayTime = currentPlayTime;
        _dateTimeProvider = dateTimeProvider;
    }

    protected override void Attach()
    {
        _startDateTime = _dateTimeProvider.Now;
    }

    public void Reset()
    {
        ThrowIfDisposed();

        _startDateTime = _dateTimeProvider.Now;
    }
}
