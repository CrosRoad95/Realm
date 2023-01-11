namespace Realm.Domain.Components.Players;

public class DiscoveriesComponent : Component
{
    private readonly HashSet<string> _discoveries = new();
    public IEnumerable<string> Discoveries => _discoveries;

    public event Action<string>? Discovered;

    public DiscoveriesComponent() { }

    public DiscoveriesComponent(ICollection<Discovery> discoveries)
    {
        _discoveries = discoveries.Select(x => x.DiscoveryId).ToHashSet();
    }

    public bool TryDiscover(string discoveryId)
    {
        if (string.IsNullOrEmpty(discoveryId))
            throw new ArgumentException(null, nameof(discoveryId));
        
        if (discoveryId.Length > 32)
            throw new ArgumentException("string must be up to 32 characters long", nameof(discoveryId));

        var success = _discoveries.Add(discoveryId);
        if (success)
            Discovered?.Invoke(discoveryId);

        return success;
    }

    public bool IsDiscovered(string discoveryId) => _discoveries.Contains(discoveryId);
}
