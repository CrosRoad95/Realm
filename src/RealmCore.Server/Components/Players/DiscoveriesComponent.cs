namespace RealmCore.Server.Components.Players;

[ComponentUsage(false)]
public class DiscoveriesComponent : Component
{
    private readonly HashSet<int> _discoveries = new();
    private readonly object _lock = new();
    public IReadOnlyCollection<int> Discoveries
    {
        get
        {
            lock (_lock)
                return new List<int>(_discoveries).AsReadOnly();
        }
    }

    public event Action<DiscoveriesComponent, int>? Discovered;

    public DiscoveriesComponent() { }

    internal DiscoveriesComponent(ICollection<DiscoveryData> discoveries)
    {
        _discoveries = discoveries.Select(x => x.DiscoveryId).ToHashSet();
    }

    public bool TryDiscover(int discoveryId)
    {
        lock (_lock)
        {
            var success = _discoveries.Add(discoveryId);
            if (success)
                Discovered?.Invoke(this, discoveryId);
            return success;
        }
    }

    public bool IsDiscovered(int discoveryId)
    {
        return _discoveries.Contains(discoveryId);
    }
}
