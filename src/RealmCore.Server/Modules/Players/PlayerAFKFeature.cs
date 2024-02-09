namespace RealmCore.Server.Modules.Players;

public interface IPlayerAFKFeature : IPlayerFeature
{
    DateTime? LastAFK { get; }
    bool IsAFK { get; }

    event Action<IPlayerAFKFeature, bool, TimeSpan>? StateChanged;

    CancellationToken CreateCancellationToken(bool? expectedAfkState = null);
    internal void HandleAFKStarted();
    internal void HandleAFKStopped();
}

internal sealed class PlayerAFKFeature : IPlayerAFKFeature
{
    private readonly object _lock = new();
    public DateTime? LastAFK { get; private set; }
    public bool IsAFK { get; private set; }
    public event Action<IPlayerAFKFeature, bool, TimeSpan>? StateChanged;
    private event Action<bool>? InternalStateChanged;
    private readonly IOptionsMonitor<GameplayOptions> _gameplayOptions;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IDebounce _debounce;
    public RealmPlayer Player { get; init; }
    public PlayerAFKFeature(PlayerContext playerContext, IOptionsMonitor<GameplayOptions> gameplayOptions, IDateTimeProvider dateTimeProvider, IDebounceFactory debounceFactory)
    {
        Player = playerContext.Player;
        _gameplayOptions = gameplayOptions;
        _dateTimeProvider = dateTimeProvider;
        _gameplayOptions.OnChange(HandleOptionsChanged);
        _debounce = debounceFactory.Create(_gameplayOptions.CurrentValue.AfkCooldown ?? 5000);
    }

    private void HandleOptionsChanged(GameplayOptions gameplayOptions)
    {
        _debounce.Milliseconds = gameplayOptions.AfkCooldown ?? 5000;
    }

    private void StateHasChanged(DateTime now)
    {
        TimeSpan elapsed = (TimeSpan)(LastAFK == null ? TimeSpan.Zero : now - LastAFK);
        StateChanged?.Invoke(this, IsAFK, elapsed);
    }

    public void HandleAFKStopped()
    {
        InternalStateChanged?.Invoke(true);
        lock (_lock)
        {
            if (IsAFK)
            {
                IsAFK = false;
                var now = _dateTimeProvider.Now;
                StateHasChanged(now);
                LastAFK = now;
            }
        }
    }

    public void HandleAFKStarted()
    {
        InternalStateChanged?.Invoke(false);
        _debounce.Invoke(() =>
        {
            lock (_lock)
            {
                if (!IsAFK)
                {
                    IsAFK = true;
                    var now = _dateTimeProvider.Now;
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
            if (expectedAfkState == null || isAfk == expectedAfkState)
            {
                cancellationTokenSource.Cancel();
                InternalStateChanged -= handleStateChanged;
            }
        }
        InternalStateChanged += handleStateChanged;
        return cancellationTokenSource.Token;
    }
}
