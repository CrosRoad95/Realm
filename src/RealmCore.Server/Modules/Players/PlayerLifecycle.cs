namespace RealmCore.Server.Modules.Players;

public abstract class PlayerLifecycle<TPlayer> where TPlayer: RealmPlayer
{
    protected readonly MtaServer _server;

    public PlayerLifecycle(MtaServer server)
    {
        server.PlayerJoined += HandlePlayerJoined;
        _server = server;
    }

    private void HandlePlayerJoined(Player plr)
    {
        var player = (TPlayer)plr;
        player.User.SignedIn += HandleSignedIn;
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
        player.User.SignedIn -= HandleSignedIn;
        player.Disconnected -= HandleDisconnected;
        PlayerLeft(player);
    }

    protected virtual void PlayerJoined(TPlayer player) { }
    protected virtual void PlayerLeft(TPlayer player) { }
    protected virtual void PlayerSignedIn(IPlayerUserFeature userService, TPlayer player) { }
}

public abstract class PlayerLifecycle : PlayerLifecycle<RealmPlayer>
{
    protected PlayerLifecycle(MtaServer server) : base(server)
    {
    }
}