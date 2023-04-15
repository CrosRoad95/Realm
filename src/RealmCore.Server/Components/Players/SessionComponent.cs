namespace RealmCore.Server.Components.Players;

public abstract class SessionComponent : Component
{
    private readonly Stopwatch _stopwatch = new();

    public event Action<Entity>? SessionStarted;
    public event Action<Entity>? SessionEnded;

    public TimeSpan Elapsed {
        get
        {
            ThrowIfDisposed();
            return _stopwatch.Elapsed;
        }
    }

    public bool IsRunning
    {
        get
        {
            ThrowIfDisposed();
            return _stopwatch.IsRunning;
        }
    }

    public SessionComponent()
    {
    }

    public void Start()
    {
        ThrowIfDisposed();

        if (IsRunning)
            throw new SessionAlreadyRunningException();
        _stopwatch.Reset();
        _stopwatch.Start();
        SessionStarted?.Invoke(Entity);
        Entity.Disposed += HandleDisposed;
    }

    private void HandleDisposed(Entity entity)
    {
        End();
    }

    public void End()
    {
        ThrowIfDisposed();

        Entity.Disposed -= HandleDisposed;
        SessionEnded?.Invoke(Entity);
        _stopwatch.Stop();
    }
}
