namespace RealmCore.TestingTools;

public abstract class RealmUnitTestingBase
{
    protected RealmTestingServer? _server;

    protected virtual RealmTestingServer CreateServer(TestConfigurationProvider? cnofiguration = null, Action<ServerBuilder>? configureBuilder = null, Action<ServiceCollection>? configureServices = null)
    {
        _server ??= new RealmTestingServer(cnofiguration ?? new TestConfigurationProvider(""), configureBuilder, services =>
            {
                services.AddRealmTestingServices(false);
                configureServices?.Invoke(services);
            });
        return _server;
    }

    protected virtual RealmPlayer CreatePlayer(string name = "TestPlayer")
    {
        if (_server == null)
            throw new Exception("Server not created.");

        var player = _server.CreatePlayer(name: name);
        if (player.IsDestroyed)
            return player;

        player.User.SignIn(new UserData
        {
            UserName = player.Name
        }, new ClaimsPrincipal
        {

        });
        return player;
    }

    protected RealmPlayer CreateServerWithOnePlayer(string name = "TestPlayer")
    {
        CreateServer();
        return CreatePlayer(name);
    }
}
