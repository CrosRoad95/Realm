using System.Diagnostics;

namespace Realm.Domain.Components.Players;

public abstract class SessionComponent : Component
{
    private readonly Stopwatch _stopwatch = new();

    public event Action<Entity>? SessionStarted;
    public event Action<Entity>? SessionEnded;

    public TimeSpan Elapsed => _stopwatch.Elapsed;

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
        Entity.Destroyed += HandleDestroyed;
    }

    private void HandleDestroyed(Entity entity)
    {
        End();
    }

    public void End()
    {
        ThrowIfDisposed();

        Entity.Destroyed -= HandleDestroyed;
        SessionEnded?.Invoke(Entity);
        _stopwatch.Stop();
    }

    public bool IsRunning => _stopwatch.IsRunning;
}
