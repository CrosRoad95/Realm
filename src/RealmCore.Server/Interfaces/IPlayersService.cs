
namespace RealmCore.Server.Interfaces;

public interface IPlayersService
{
    event Action<RealmPlayer>? PlayerLoaded;

    internal void RelayLoaded(RealmPlayer player);
}
