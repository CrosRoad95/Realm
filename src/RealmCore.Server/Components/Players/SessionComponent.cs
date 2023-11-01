namespace RealmCore.Server.Components.Players;

public abstract class SessionComponent : ComponentLifecycle
{
    private readonly Stopwatch _stopwatch = new();

    public event Action<SessionComponent>? SessionStarted;
    public event Action<SessionComponent>? SessionEnded;

    public TimeSpan Elapsed => _stopwatch.Elapsed;

    public bool IsRunning => _stopwatch.IsRunning;

    public SessionComponent()
    {
    }

    public void Start()
    {
        if (IsRunning)
            throw new SessionAlreadyRunningException();
        _stopwatch.Reset();
        _stopwatch.Start();
        SessionStarted?.Invoke(this);
    }

    public override void Detach()
    {
        End();
    }

    public void End()
    {
        SessionEnded?.Invoke(this);
        _stopwatch.Stop();
    }
}
