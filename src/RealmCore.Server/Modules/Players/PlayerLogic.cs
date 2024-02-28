namespace RealmCore.Server.Modules.Players;

public abstract class PlayerLogic
{
    protected readonly MtaServer _server;

    public PlayerLogic(MtaServer server)
    {
        server.PlayerJoined += HandlePlayerJoined;
        _server = server;
    }

    private void HandlePlayerJoined(Player plr)
    {
        var player = (RealmPlayer)plr;
        player.User.SignedIn += HandleSignedIn;
        player.Disconnected += HandleDisconnected;
        PlayerJoined(player);
    }

    private void HandleDisconnected(Player plr, PlayerQuitEventArgs e)
    {
        var player = (RealmPlayer)plr;
        player.User.SignedIn -= HandleSignedIn;
        player.Disconnected -= HandleDisconnected;
        PlayerLeft(player);
    }

    protected virtual void PlayerJoined(RealmPlayer player) { }
    protected virtual void PlayerLeft(RealmPlayer player) { }
    protected virtual void HandleSignedIn(IPlayerUserFeature userService, RealmPlayer player) { }
}
