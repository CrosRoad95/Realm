namespace RealmCore.Server.Abstractions;

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
    }

    private void HandleDisconnected(Player plr, PlayerQuitEventArgs e)
    {
        var player = (RealmPlayer)plr;
        player.User.SignedIn -= HandleSignedIn;
    }

    protected abstract void HandleSignedIn(IPlayerUserService userService, RealmPlayer player);
}
