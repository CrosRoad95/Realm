namespace RealmCore.Server.Components.Players;

[ComponentUsage(false)]
public class AFKComponent : Component
{
    public DateTime? LastAFK { get; private set; }
    public bool IsAFK { get; private set; }
    public event Action<AFKComponent, bool, TimeSpan>? StateChanged;
    private object _lock = new();

    protected virtual void StateHasChanged(DateTime now)
    {
        TimeSpan elapsed = (TimeSpan)(LastAFK == null ? TimeSpan.Zero : now - LastAFK);
        StateChanged?.Invoke(this, IsAFK, elapsed);
    }

    internal void HandlePlayerAFKStopped(DateTime now)
    {
        lock (_lock)
        {
            IsAFK = false;
            StateHasChanged(now);
            LastAFK = now;
        }
    }

    internal void HandlePlayerAFKStarted(DateTime now)
    {
        lock (_lock)
        {
            IsAFK = true;
            StateHasChanged(now);
            LastAFK = now;
        }
    }
}
