using Realm.Server.Interfaces;

namespace Realm.Server;

internal class Startup
{
    private readonly ITestRepository _testRepository;
    private readonly IRPGServer _server;
    private readonly IResourceProvider _resourceProvider;

    public Startup(ITestRepository testRepository, IRPGServer server, IResourceProvider resourceProvider)
    {
        _testRepository = testRepository;
        _server = server;
        _resourceProvider = resourceProvider;
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
