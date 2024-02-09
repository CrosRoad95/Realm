namespace RealmCore.Server.Modules.Players;

public interface IPlayerEventManager
{
    event Action<RealmPlayer>? PlayerLoaded;

    internal void RelayLoaded(RealmPlayer player);
}

internal sealed class PlayerEventManager : IPlayerEventManager
{
    public event Action<RealmPlayer>? PlayerLoaded;

    public void RelayLoaded(RealmPlayer player)
    {
        PlayerLoaded?.Invoke(player);
    }
}
