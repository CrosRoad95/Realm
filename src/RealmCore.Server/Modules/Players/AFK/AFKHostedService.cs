namespace RealmCore.Server.Modules.Players.AFK;

internal sealed class AFKHostedService : PlayerLifecycle, IHostedService
{
    private readonly IElementCollection _elementCollection;
    private readonly IOptionsMonitor<GameplayOptions> _gameplayOptions;
    private readonly ILogger<AFKHostedService> _logger;
    private int _afkCooldown;

    private const int _defaultAfkCooldown = 5000;
    public AFKHostedService(PlayersEventManager playersEventManager, IElementCollection elementCollection, IOptionsMonitor<GameplayOptions> gameplayOptions, ILogger<AFKHostedService> logger) : base(playersEventManager)
    {
        _elementCollection = elementCollection;
        _gameplayOptions = gameplayOptions;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _gameplayOptions.OnChange(HandleGameplayOptionsChanged);
        HandleGameplayOptionsChanged(_gameplayOptions.CurrentValue);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void HandleGameplayOptionsChanged(GameplayOptions gameplayOptions)
    {
        if(gameplayOptions.AfkCooldown == _afkCooldown)
            return;

        _afkCooldown = Math.Max(gameplayOptions.AfkCooldown ?? _defaultAfkCooldown, _defaultAfkCooldown);

        foreach (var player in _elementCollection.GetByType<RealmPlayer>())
            player.AFK.SetCooldown(_afkCooldown);

        _logger.LogInformation("Set afk cooldown to: {miliseconds}", _afkCooldown);
    }

    protected override void PlayerJoined(RealmPlayer player)
    {
        player.AFK.SetCooldown(_afkCooldown);
    }
}
