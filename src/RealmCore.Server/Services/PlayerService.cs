namespace RealmCore.Server.Services;

public interface IPlayersService
{
    event Action<RealmPlayer>? PlayerLoaded;

    internal void RelayLoaded(RealmPlayer player);
}

internal sealed class PlayersService : IPlayersService
{
    public event Action<RealmPlayer>? PlayerLoaded;

    public void RelayLoaded(RealmPlayer player)
    {
        PlayerLoaded?.Invoke(player);
    }
}
