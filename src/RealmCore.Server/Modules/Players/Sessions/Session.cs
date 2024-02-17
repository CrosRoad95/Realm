namespace RealmCore.Server.Modules.Players.Sessions;

public abstract class Session
{
    public bool _ended;
    protected readonly object _lock = new();
    private readonly Stopwatch _stopwatch = new();

    public event Action<Session>? Started;
    public event Action<Session>? Ended;

    public TimeSpan Elapsed => _stopwatch.Elapsed;

    public bool IsRunning => _stopwatch.IsRunning;
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
