namespace RealmCore.Server.Modules.Players;

public sealed class PlayerDiscoveriesFeature : IPlayerFeature, IEnumerable<int>, IUsesUserPersistentData
{
    private readonly object _lock = new();
    private ICollection<DiscoveryData> _discoveries = [];

    public event Action<PlayerDiscoveriesFeature, int>? Discovered;
    public event Action? VersionIncreased;

    public RealmPlayer Player { get; init; }
    public PlayerDiscoveriesFeature(PlayerContext playerContext)
    {
        Player = playerContext.Player;
    }

    public void LogIn(UserData userData)
    {
        lock (_lock)
            _discoveries = userData.Discoveries;
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
        }

        VersionIncreased?.Invoke();
        Discovered?.Invoke(this, discoveryId);
        return true;
    }

    public bool IsDiscovered(int discoveryId)
    {
        lock (_lock)
            return InternalIsDiscovered(discoveryId);
    }

    public IEnumerator<int> GetEnumerator()
    {
        int[] view;
        lock (_lock)
            view = [.. _discoveries.Select(x => x.DiscoveryId)];

        foreach (var discoveryId in view)
        {
            yield return discoveryId;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
