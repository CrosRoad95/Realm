namespace RealmCore.Server.Modules.Players;

public sealed class PlayerContext
{
    private RealmPlayer? _player;

    public RealmPlayer Player { get => _player ?? throw new InvalidPlayerContextException(); internal set => _player = value; }

    public void Set(RealmPlayer realmPlayer)
    {
        if (Player != null)
            throw new InvalidOperationException("Player already set");

        Player = realmPlayer;
    }
}
