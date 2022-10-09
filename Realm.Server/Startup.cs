namespace Realm.Server;

internal class Startup
{
    private readonly ITestRepository _testRepository;
    private readonly IRPGServer _server;
    private readonly IResourceProvider _resourceProvider;
    private readonly IEnumerable<IAutoStartResource> _autoStartResources;
    private readonly IEnumerable<IAsyncService> _asyncServices;

    public Startup(ITestRepository testRepository, IRPGServer server, IResourceProvider resourceProvider, IEnumerable<IAutoStartResource> autoStartResources, IEnumerable<IAsyncService> asyncServices)
    {
        _testRepository = testRepository;
        _server = server;
        _resourceProvider = resourceProvider;
        _autoStartResources = autoStartResources;
        _asyncServices = asyncServices;
        server.PlayerJoined += Server_PlayerJoined;
    }

    private void Server_PlayerJoined(IRPGPlayer player)
    {
        foreach (var resource in _autoStartResources)
            resource.StartFor(player);

    }

    public async Task StartAsync()
    {
        foreach (var asyncService in _asyncServices)
            await asyncService.StartAsync();

        await _testRepository.AddTest(new Persistance.Entities.Test
        {
            Number = 123,
            Text = "Realm",
        });

    }
}
