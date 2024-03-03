namespace RealmCore.Server.Modules.Players;

public interface IPlayersEventManager
{
    event Action<Player>? PlayerLoaded;

    void RelayLoaded(Player player);
}

internal sealed class PlayerEventManager : IPlayersEventManager
{
    public event Action<Player>? PlayerLoaded;

    public void RelayLoaded(Player player)
    {
        PlayerLoaded?.Invoke(player);
    }
}
