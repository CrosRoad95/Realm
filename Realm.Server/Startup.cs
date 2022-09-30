namespace Realm.Server;

internal class Startup
{
    private readonly ITestRepository _testRepository;
    private readonly MtaServer<DefaultPlayer> _server;
    private readonly IResourceProvider _resourceProvider;
    private readonly IEnumerable<IAutoStartResource> _autoStartResources;

    public Startup(ITestRepository testRepository, MtaServer<DefaultPlayer> server, IResourceProvider resourceProvider, IEnumerable<IAutoStartResource> autoStartResources)
    {
        _testRepository = testRepository;
        _server = server;
        _resourceProvider = resourceProvider;
        _autoStartResources = autoStartResources;
        server.PlayerJoined += Server_PlayerJoined;
    }

    private void Server_PlayerJoined(DefaultPlayer player)
    {
        foreach (var resource in _autoStartResources)
            resource.StartFor(_resourceProvider, player);

    }

    public async Task StartAsync()
    {
        await _testRepository.AddTest(new Persistance.Entities.Test
        {
            Number = 123,
            Text = "Realm",
        });

    }
}
