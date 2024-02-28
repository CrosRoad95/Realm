namespace RealmCore.Server.Modules.Players;

public interface IPlayerDiscoveriesFeature : IPlayerFeature, IEnumerable<int>
{
    event Action<IPlayerDiscoveriesFeature, int>? Discovered;

    bool IsDiscovered(int discoveryId);
    bool TryDiscover(int discoveryId);
}

internal sealed class PlayerDiscoveriesFeature : IPlayerDiscoveriesFeature, IUsesUserPersistentData
{
    private readonly object _lock = new();
    private ICollection<DiscoveryData> _discoveries = [];

    public event Action<IPlayerDiscoveriesFeature, int>? Discovered;
    public event Action? VersionIncreased;

    public RealmPlayer Player { get; init; }
    public PlayerDiscoveriesFeature(PlayerContext playerContext)
    {
        Player = playerContext.Player;
    }

    public void SignIn(UserData userData)
    {
        lock (_lock)
            _discoveries = userData.Discoveries;
    }

    public void SignOut()
    {
        lock (_lock)
            _discoveries = [];
    }

    private bool InternalIsDiscovered(int discoveryId) => _discoveries.Any(x => x.DiscoveryId == discoveryId);

    public bool TryDiscover(int discoveryId)
    {
        lock (_lock)
        {
            if (InternalIsDiscovered(discoveryId))
                return false;
            var discoveryData = new DiscoveryData
            {
                DiscoveryId = discoveryId
            };
            _discoveries.Add(discoveryData);
            VersionIncreased?.Invoke();
            Discovered?.Invoke(this, discoveryId);
            return true;
        }
    }

    public bool IsDiscovered(int discoveryId)
    {
        lock (_lock)
            return InternalIsDiscovered(discoveryId);
    }

    public IEnumerator<int> GetEnumerator()
    {
        lock (_lock)
            return _discoveries.Select(x => x.DiscoveryId).ToList().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
