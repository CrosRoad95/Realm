namespace RealmCore.Server.Modules.Serving;

internal sealed class RealmResourceServer : IResourceServer
{
    public static int _resourceCounter = 0;
    private readonly IResourceServer _resourceServer;

    public RealmResourceServer(IResourceServer resourceServer)
    {
        _resourceServer = resourceServer;
    }

    public void AddAdditionalResource(Resource resource, Dictionary<string, byte[]> files)
    {
        _resourceCounter++;
        _resourceServer.AddAdditionalResource(resource, files);
    }

    public void RemoveAdditionalResource(Resource resource)
    {
        _resourceCounter--;
        _resourceServer.RemoveAdditionalResource(resource);
    }

    public void Start() => _resourceServer.Start();

    public void Stop() => _resourceServer.Stop();
}
