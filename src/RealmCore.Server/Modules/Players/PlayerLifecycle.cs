namespace RealmCore.Server.Modules.Players;

public abstract class PlayerLifecycle<TPlayer> where TPlayer: RealmPlayer
{
    protected readonly PlayersEventManager _playersEventManager;

    public PlayerLifecycle(PlayersEventManager playersEventManager)
    {
        playersEventManager.Joined += HandlePlayerJoined;
        playersEventManager.Spawned += HandlePlayerSpawned;
        playersEventManager.Loaded += HandleLoaded;
        _playersEventManager = playersEventManager;
    }

    private void HandleLoaded(Player plr)
    {
        var player = (TPlayer)plr;
        PlayerLoaded(player);
    }

    private void HandlePlayerSpawned(Player plr)
    {
        var player = (TPlayer)plr;
        PlayerSpawned(player);
    }

    private void HandlePlayerJoined(Player plr)
    {
        var player = (TPlayer)plr;
        player.User.LoggedIn += HandleLoggedIn;
        player.Disconnected += HandleDisconnected;
        PlayerJoined(player);
    }

    private async Task HandleLoggedIn(object? sender, PlayerLoggedInEventArgs args)
    {
        await PlayerLoggedIn(args.PlayerUserFeature, (TPlayer)args.Player);
    }

    private void HandleDisconnected(Player plr, PlayerQuitEventArgs e)
    {
        var player = (TPlayer)plr;
        player.User.LoggedIn -= HandleLoggedIn;
        player.Disconnected -= HandleDisconnected;
        PlayerLeft(player);
    }

    protected virtual void PlayerJoined(TPlayer player) { }
    protected virtual void PlayerLeft(TPlayer player) { }
    protected virtual Task PlayerLoggedIn(IPlayerUserFeature user, TPlayer player) { return Task.CompletedTask; }
    protected virtual void PlayerSpawned(TPlayer player) { }
    protected virtual void PlayerLoaded(TPlayer player) { }
}

public abstract class PlayerLifecycle : PlayerLifecycle<RealmPlayer>
{
    protected PlayerLifecycle(PlayersEventManager playersEventManager) : base(playersEventManager)
    {
    }
}