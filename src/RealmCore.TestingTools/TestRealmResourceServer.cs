namespace RealmCore.TestingTools;

public sealed class TestRealmResourceServer : IResourceServer
{
    private readonly IRealmResourcesProvider _realmResourcesProvider;

    public TestRealmResourceServer(IRealmResourcesProvider realmResourcesProvider)
    {
        _realmResourcesProvider = realmResourcesProvider;
    }
    public void AddAdditionalResource(Resource resource, Dictionary<string, byte[]> files)
    {
        _realmResourcesProvider.Add(resource);
    }

    public void RemoveAdditionalResource(Resource resource)
    {
        _realmResourcesProvider.Remove(resource);
    }

    public void Start() { }

    public void Stop() { }
}
