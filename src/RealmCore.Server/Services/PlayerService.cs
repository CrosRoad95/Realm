namespace RealmCore.Server.Services;

internal sealed class PlayersService : IPlayersService
{
    public event Action<RealmPlayer>? PlayerLoaded;

    public void RelayLoaded(RealmPlayer player)
    {
        PlayerLoaded?.Invoke(player);
    }
}
