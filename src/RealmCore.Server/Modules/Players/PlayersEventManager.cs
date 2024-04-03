namespace RealmCore.Server.Modules.Players;

public sealed class PlayersEventManager
{
    public event Action<Player>? Joined;
    public event Action<Player>? Loaded;
    public event Action<Player>? Unloading;
    public event Action<Player>? Spawned;

    public void RelayJoined(Player player)
    {
        Joined?.Invoke(player);
    }

    public void RelayLoaded(Player player)
    {
        Loaded?.Invoke(player);
    }

    public void RelayUnloading(Player player)
    {
        Unloading?.Invoke(player);
    }

    public void RelaySpawned(Player player)
    {
        Spawned?.Invoke(player);
    }
}
