using Realm.Persistance.Data;
using Realm.Persistance.Interfaces;

namespace Realm.Server;

internal class Startup
{
    private readonly ITestRepository _testRepository;
    private readonly IRPGServer _server;
    private readonly IResourceProvider _resourceProvider;
    private readonly SignInManager<User> _signInManager;

    public Startup(ITestRepository testRepository, IRPGServer server, IResourceProvider resourceProvider, SignInManager<User> signInManager)
    {
        _testRepository = testRepository;
        _server = server;
        _resourceProvider = resourceProvider;
        _signInManager = signInManager;
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
