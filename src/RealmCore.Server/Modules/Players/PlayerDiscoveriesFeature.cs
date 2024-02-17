namespace RealmCore.Server.Modules.Players;

public interface IPlayerDiscoveriesFeature : IPlayerFeature, IEnumerable<int>
{
    event Action<IPlayerDiscoveriesFeature, int>? Discovered;

    bool IsDiscovered(int discoveryId);
    bool TryDiscover(int discoveryId);
}

internal sealed class PlayerDiscoveriesFeature : IPlayerDiscoveriesFeature
{
    private readonly object _lock = new();
    private ICollection<DiscoveryData> _discoveries = [];
    private readonly IPlayerUserFeature _playerUserFeature;

    public event Action<IPlayerDiscoveriesFeature, int>? Discovered;

    public RealmPlayer Player { get; init; }
    public PlayerDiscoveriesFeature(PlayerContext playerContext, IPlayerUserFeature playerUserFeature)
    {
        Player = playerContext.Player;
        playerUserFeature.SignedIn += HandleSignedIn;
        playerUserFeature.SignedOut += HandleSignedOut;
        _playerUserFeature = playerUserFeature;
    }

    private void HandleSignedIn(IPlayerUserFeature playerUserFeature, RealmPlayer _)
    {
        lock (_lock)
            _discoveries = playerUserFeature.UserData.Discoveries;
    }

    private void HandleSignedOut(IPlayerUserFeature playerUserFeature, RealmPlayer _)
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
            _playerUserFeature.IncreaseVersion();
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
