namespace RealmCore.Server.Components.Players;

[ComponentUsage(false)]
public class AFKComponent : Component
{
    public DateTime? LastAFK { get; private set; }
    public bool IsAFK { get; private set; }
    public event Action<AFKComponent, bool, TimeSpan>? StateChanged;
    private event Action<bool>? InternalStateChanged;
    private readonly IOptionsMonitor<GameplayOptions> _gameplayOptions;
    private readonly Debounce _debounce;
    private readonly object _lock = new();

    public AFKComponent(IOptionsMonitor<GameplayOptions> gameplayOptions)
    {
        _gameplayOptions = gameplayOptions;
        _gameplayOptions.OnChange(HandleOptionsChanged);
        _debounce = new(_gameplayOptions.CurrentValue.AfkCooldown ?? 5000);
    }

    private void HandleOptionsChanged(GameplayOptions gameplayOptions)
    {
        _debounce.Milliseconds = gameplayOptions.AfkCooldown ?? 5000;
    }

    protected virtual void StateHasChanged(DateTime now)
    {
        TimeSpan elapsed = (TimeSpan)(LastAFK == null ? TimeSpan.Zero : now - LastAFK);
        StateChanged?.Invoke(this, IsAFK, elapsed);
    }

    internal void HandlePlayerAFKStopped(DateTime now)
    {
        InternalStateChanged?.Invoke(true);
        lock (_lock)
        {
            if (IsAFK)
            {
                IsAFK = false;
                StateHasChanged(now);
                LastAFK = now;
            }
        }
    }

    internal void HandlePlayerAFKStarted(DateTime now)
    {
        InternalStateChanged?.Invoke(false);
        _debounce.Invoke(() =>
        {
            lock (_lock)
            {
                if (!IsAFK)
                {
                    IsAFK = true;
                    StateHasChanged(now);
                    LastAFK = now;
                }
            }
        }, CreateCancellationToken());
    }

    /// <summary>
    /// Cancelled when expected afk state is afk or not or both
    /// </summary>
    /// <returns></returns>
    public CancellationToken CreateCancellationToken(bool? expectedAfkState = null)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        void handleStateChanged(bool isAfk)
        {
            if(expectedAfkState == null || isAfk == expectedAfkState)
            {
                cancellationTokenSource.Cancel();
                InternalStateChanged -= handleStateChanged;
            }
        }
        InternalStateChanged += handleStateChanged;
        return cancellationTokenSource.Token;
    }
}
