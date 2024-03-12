namespace RealmCore.Server.Modules.Players;

public interface IPlayersEventManager
{
    event Action<Player>? PlayerLoaded;
    event Action<Player>? PlayerUnloading;
    event Action<Player>? PlayerJoined;

    void RelayJoined(Player player);
    void RelayLoaded(Player player);
    void RelayUnloading(Player player);
}

internal sealed class PlayerEventManager : IPlayersEventManager
{
    public event Action<Player>? PlayerJoined;
    public event Action<Player>? PlayerLoaded;
    public event Action<Player>? PlayerUnloading;

    public void RelayJoined(Player player)
    {
        PlayerJoined?.Invoke(player);
    }

    public void RelayLoaded(Player player)
    {
        PlayerLoaded?.Invoke(player);
    }

    public void RelayUnloading(Player player)
    {
        PlayerUnloading?.Invoke(player);
    }
}
