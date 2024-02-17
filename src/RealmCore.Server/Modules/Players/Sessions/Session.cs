namespace RealmCore.Server.Modules.Players.Sessions;

public abstract class Session : IDisposable
{
    public AtomicBool _disposed;
    protected readonly object _lock = new();
    private readonly TimeMeasurement _timeMeasurement;

    public event Action<Session>? Started;
    public event Action<Session>? Ended;

    public TimeSpan Elapsed => _timeMeasurement.Elapsed;
    public bool IsRunning => _timeMeasurement.IsRunning;
    public RealmPlayer Player { get; private set; }
    private bool _firstTime = true;

    public Session(RealmPlayer player, IDateTimeProvider dateTimeProvider)
    {
        Player = player;
        _timeMeasurement = new(dateTimeProvider);
    }

    protected virtual void OnStarted() { }
    protected virtual void OnEnded() { }

    public void TryStart()
    {
        if (_disposed)
            throw new SessionAlreadyEndedException(this);

        if (_timeMeasurement.TryStart())
        {
            Started?.Invoke(this);
            if (_firstTime)
            {
                _firstTime = false;
                OnStarted();
            }
        }
    }

    public bool TryStop()
    {
        if (_disposed)
            throw new SessionAlreadyEndedException(this);

        return _timeMeasurement.TryStop();
    }

    public void Dispose()
    {
        if (!_disposed.TrySetTrue())
            throw new SessionAlreadyEndedException(this);

        lock (_lock)
        {
            _timeMeasurement.TryStop();
            Ended?.Invoke(this);
            OnEnded();
        }
    }
}
