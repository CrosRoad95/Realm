namespace RealmCore.Server.Modules.Players;

public interface IPlayersEventManager
{
    event Action<RealmPlayer>? PlayerLoaded;

    void RelayLoaded(RealmPlayer player);
}

internal sealed class PlayerEventManager : IPlayersEventManager
{
    public event Action<RealmPlayer>? PlayerLoaded;

    public void RelayLoaded(RealmPlayer player)
    {
        PlayerLoaded?.Invoke(player);
    }
}
