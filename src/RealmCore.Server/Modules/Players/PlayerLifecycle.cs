namespace RealmCore.Server.Modules.Players;

public abstract class PlayerLifecycle<TPlayer> where TPlayer: RealmPlayer
{
    protected readonly PlayersEventManager _playersEventManager;

    public PlayerLifecycle(PlayersEventManager playersEventManager)
    {
        playersEventManager.Joined += HandlePlayerJoined;
        playersEventManager.Spawned += HandlePlayerSpawned;
        _playersEventManager = playersEventManager;
    }

    private void HandlePlayerSpawned(Player plr)
    {
        var player = (TPlayer)plr;
        PlayerSpawned(player);
    }

    private void HandlePlayerJoined(Player plr)
    {
        var player = (TPlayer)plr;
        player.User.LoggedIn += HandleSignedIn;
        player.Disconnected += HandleDisconnected;
        PlayerJoined(player);
    }

    private void HandleSignedIn(IPlayerUserFeature user, RealmPlayer player)
    {
        PlayerSignedIn(user, (TPlayer)player);
    }

    private void HandleDisconnected(Player plr, PlayerQuitEventArgs e)
    {
        var player = (TPlayer)plr;
        player.User.LoggedIn -= HandleSignedIn;
        player.Disconnected -= HandleDisconnected;
        PlayerLeft(player);
    }

    protected virtual void PlayerJoined(TPlayer player) { }
    protected virtual void PlayerLeft(TPlayer player) { }
    protected virtual void PlayerSignedIn(IPlayerUserFeature userService, TPlayer player) { }
    protected virtual void PlayerSpawned(TPlayer player) { }
}

public abstract class PlayerLifecycle : PlayerLifecycle<RealmPlayer>
{
    protected PlayerLifecycle(PlayersEventManager playersEventManager) : base(playersEventManager)
    {
    }
}