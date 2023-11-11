namespace RealmCore.Server.Abstractions;

public class PlayerLogic
{
    public PlayerLogic(MtaServer mtaServer)
    {
        mtaServer.PlayerJoined += HandlePlayerJoined;
    }

    private void HandlePlayerJoined(Player plr)
    {
        var player = (RealmPlayer)plr;
        player.User.SignedIn += HandleSignedIn;
    }

    protected virtual void HandleSignedIn(IPlayerUserService userService, RealmPlayer player) { }
}
