namespace RealmCore.Server.Concepts.Sessions;

public abstract class Session
{
    private readonly Stopwatch _stopwatch = new();

    public event Action<Session>? Started;
    public event Action<Session>? Ended;

    public TimeSpan Elapsed => _stopwatch.Elapsed;

    public bool IsRunning => _stopwatch.IsRunning;
    private object _lock = new();
    public RealmPlayer Player { get; private set; }

    public Session(RealmPlayer player)
    {
        Player = player;
    }

    protected virtual void OnStarted() { }
    protected virtual void OnEnded() { }
    public void Start()
    {
        lock (_lock)
        {
            if (IsRunning)
                throw new SessionAlreadyRunningException();
            _stopwatch.Reset();
            _stopwatch.Start();
            OnStarted();
            Started?.Invoke(this);
        }
    }

    public void Pause()
    {
        lock (_lock)
        {
            _stopwatch.Stop();
        }
    }

    public void Continue()
    {
        lock (_lock)
        {
            _stopwatch.Start();
        }
    }

    public void End()
    {
        lock (_lock)
        {
            _stopwatch.Stop();
            OnEnded();
            Ended?.Invoke(this);
        }
    }
}
