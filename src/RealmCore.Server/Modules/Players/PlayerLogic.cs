namespace RealmCore.Server.Modules.Players;

public abstract class PlayerLogic
{
    public PlayerLogic(MtaServer mtaServer)
    {
        mtaServer.PlayerJoined += HandlePlayerJoined;
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

    protected abstract void PlayerJoined(RealmPlayer player);
    protected abstract void PlayerLeft(RealmPlayer player);
    protected virtual void HandleSignedIn(IPlayerUserFeature userService, RealmPlayer player) { }
}
