namespace RealmCore.Server.Modules.Serving;

public interface IRealmResourcesProvider
{
    int Count { get; }
    IEnumerable<Resource> All { get; }

    void Add(Resource resource);
    void Remove(Resource resource);
}

internal sealed class RealmResourcesProvider : IRealmResourcesProvider
{
    private readonly List<Resource> _resources = [];

    public int Count => _resources.Count;
    public IEnumerable<Resource> All => _resources;

    public void Add(Resource resource)
    {
        _resources.Add(resource);
    }

    public void Remove(Resource resource)
    {
        _resources.Remove(resource);
    }
}

internal sealed class RealmResourceServer : IResourceServer
{
    private readonly IResourceServer _resourceServer;
    private readonly IRealmResourcesProvider _realmResourcesProvider;

    public RealmResourceServer(IResourceServer resourceServer, IRealmResourcesProvider realmResourcesProvider)
    {
        _resourceServer = resourceServer;
        _realmResourcesProvider = realmResourcesProvider;
    }

    public void AddAdditionalResource(Resource resource, Dictionary<string, byte[]> files)
    { 
        _realmResourcesProvider.Add(resource);
        _resourceServer.AddAdditionalResource(resource, files);
    }

    public void RemoveAdditionalResource(Resource resource)
    {
        _realmResourcesProvider.Remove(resource);
        _resourceServer.RemoveAdditionalResource(resource);
    }

    public void Start() { }

    public void Stop() { }
}
