namespace RealmCore.Server.Modules.Players;

public sealed class PlayerContext
{
    public RealmPlayer Player { get; internal set; }

    public void Set(RealmPlayer realmPlayer)
    {
        if (Player != null)
            throw new InvalidOperationException();

        Player = realmPlayer;
    }
}
