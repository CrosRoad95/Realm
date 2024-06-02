namespace RealmCore.TestingTools;

public sealed class TestRealmResourceServer : IResourceServer
{
    public List<Resource> Resources = [];
    public void AddAdditionalResource(Resource resource, Dictionary<string, byte[]> files)
    {
        RealmResourceServer._resourceCounter++;
        Resources.Add(resource);
    }

    public void RemoveAdditionalResource(Resource resource)
    {
        RealmResourceServer._resourceCounter--;
    }

    public void Start() { }

    public void Stop() { }
}
