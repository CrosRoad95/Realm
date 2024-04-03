namespace RealmCore.Server.Modules.Players.AFK;

internal sealed class AFKLogic : PlayerLifecycle
{
    private readonly IElementCollection _elementCollection;
    private readonly IOptionsMonitor<GameplayOptions> _gameplayOptions;
    private readonly ILogger<AFKLogic> _logger;
    private int _afkCooldown;

    private const int _defaultAfkCooldown = 5000;
    public AFKLogic(PlayersEventManager playersEventManager, IElementCollection elementCollection, IOptionsMonitor<GameplayOptions> gameplayOptions, ILogger<AFKLogic> logger) : base(playersEventManager)
    {
        gameplayOptions.OnChange(HandleGameplayOptionsChanged);
        _elementCollection = elementCollection;
        _gameplayOptions = gameplayOptions;
        _logger = logger;
        _afkCooldown = gameplayOptions.CurrentValue.AfkCooldown ?? _defaultAfkCooldown;
        _logger.LogInformation("Set afk cooldown to: {miliseconds}", _afkCooldown);
    }

    private void HandleGameplayOptionsChanged(GameplayOptions gameplayOptions, string? asd)
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
