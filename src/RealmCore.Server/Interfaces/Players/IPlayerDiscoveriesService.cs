
namespace RealmCore.Server.Interfaces.Players;

public interface IPlayerDiscoveriesService : IPlayerService, IEnumerable<int>
{
    event Action<IPlayerDiscoveriesService, int>? Discovered;

    bool IsDiscovered(int discoveryId);
    bool TryDiscover(int discoveryId);
}
