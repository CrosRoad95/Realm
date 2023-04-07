namespace RealmCore.Server.Components.Players;

[ComponentUsage(false)]
public class DiscoveriesComponent : Component
{
    private readonly HashSet<int> _discoveries = new();
    private readonly object _discoveriesLock = new();
    public IReadOnlyCollection<int> Discoveries => _discoveries;

    public event Action<DiscoveriesComponent, int>? Discovered;

    public DiscoveriesComponent() { }

    internal DiscoveriesComponent(ICollection<DiscoveryData> discoveries)
    {
        _discoveries = discoveries.Select(x => x.DiscoveryId).ToHashSet();
    }

    public bool TryDiscover(int discoveryId)
    {
        ThrowIfDisposed();

        lock (_discoveriesLock)
        {
            var success = _discoveries.Add(discoveryId);
            if (success)
                Discovered?.Invoke(this, discoveryId);
            return success;
        }
    }

    public bool IsDiscovered(int discoveryId)
    {
        ThrowIfDisposed();
        return _discoveries.Contains(discoveryId);
    }
}
