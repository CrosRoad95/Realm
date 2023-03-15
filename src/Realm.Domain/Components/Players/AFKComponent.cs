namespace Realm.Domain.Components.Players;

[ComponentUsage(false)]
public class AFKComponent : Component
{
    [Inject]
    private IDateTimeProvider DateTimeProvider { get; set; } = default!;

    public DateTime? LastAFK { get; private set; }
    public bool IsAFK { get; private set; }
    public event Action<AFKComponent, bool, TimeSpan>? StateChanged;
    private object _lock = new();

    protected virtual void StateHasChanged()
    {
        TimeSpan elapsed = (TimeSpan)((LastAFK == null) ? TimeSpan.Zero : DateTimeProvider.Now - LastAFK);
        StateChanged?.Invoke(this, IsAFK, elapsed);
    }

    internal void HandlePlayerAFKStopped()
    {
        lock (_lock)
        {
            IsAFK = false;
            StateHasChanged();
            LastAFK = DateTimeProvider.Now;
        }
    }

    internal void HandlePlayerAFKStarted()
    {
        lock(_lock)
        {
            IsAFK = true;
            StateHasChanged();
            LastAFK = DateTimeProvider.Now;
        }
    }
}
