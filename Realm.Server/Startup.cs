namespace Realm.Server;

internal class Startup
{
    private readonly IRPGServer _server;
    private readonly IResourceProvider _resourceProvider;
    private readonly SignInManager<User> _signInManager;

    public Startup(IRPGServer server, IResourceProvider resourceProvider, SignInManager<User> signInManager)
    {
        _server = server;
        _resourceProvider = resourceProvider;
        _signInManager = signInManager;
    }

    public async Task StartAsync()
    {
    }
}
