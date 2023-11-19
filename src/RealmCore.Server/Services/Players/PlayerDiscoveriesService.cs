namespace RealmCore.Server.Services.Players;

internal class PlayerDiscoveriesService : IPlayerDiscoveriesService
{
    private ICollection<DiscoveryData> _discoveries = [];
    private readonly object _lock = new();
    private readonly IPlayerUserService _playerUserService;

    public event Action<IPlayerDiscoveriesService, int>? Discovered;

    public RealmPlayer Player { get; private set; }
    public PlayerDiscoveriesService(PlayerContext playerContext, IPlayerUserService playerUserService)
    {
        Player = playerContext.Player;
        playerUserService.SignedIn += HandleSignedIn;
        playerUserService.SignedOut += HandleSignedOut;
        _playerUserService = playerUserService;
    }

    private void HandleSignedIn(IPlayerUserService playerUserService, RealmPlayer _)
    {
        lock (_lock)
            _discoveries = playerUserService.User.Discoveries;
    }

    private void HandleSignedOut(IPlayerUserService playerUserService, RealmPlayer _)
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
            _playerUserService.IncreaseVersion();
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
