using System.Diagnostics;

namespace Realm.Domain.Components.Players;

public abstract class SessionComponent : Component
{
    private Stopwatch _stopwatch = new();

    public event Action<Entity>? SessionStarted;
    public event Action<Entity>? SessionEnded;

    public TimeSpan Elapsed => _stopwatch.Elapsed;

    public SessionComponent()
    {
    }

    public void Start()
    {
        _stopwatch.Reset();
        _stopwatch.Start();
        SessionStarted?.Invoke(Entity);
    }

    public void End()
    {
        SessionEnded?.Invoke(Entity);
        _stopwatch.Stop();
    }

    public bool IsRunning => _stopwatch.IsRunning;
}
