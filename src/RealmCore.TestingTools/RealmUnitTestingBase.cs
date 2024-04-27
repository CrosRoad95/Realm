using SlipeServer.Server.Behaviour;

namespace RealmCore.TestingTools;

public abstract class RealmUnitTestingBase<TRealmTestingServer, TRealmPlayer> : RealmTestingBase<TRealmTestingServer, TRealmPlayer>
    where TRealmTestingServer : RealmTestingServer<TRealmPlayer>
    where TRealmPlayer : Player
{
    protected override TRealmPlayer CreatePlayer(string name = "TestPlayer")
    {
        if (_server == null)
            throw new Exception("Server not created.");

        var player = _server.CreatePlayer(name: name);
        if (player.IsDestroyed)
            return player;

        if(player is RealmPlayer realmPlayer)
        {
            realmPlayer.User.SignIn(new UserData
            {
                UserName = player.Name
            }, new ClaimsPrincipal
            {

            });

            realmPlayer.User.TryFlushVersion(0);
        }
        return player;
    }

    protected TRealmPlayer CreateServerWithOnePlayer(string name = "TestPlayer")
    {
        CreateServer();
        return CreatePlayer(name);
    }
}

public abstract class RealmUnitTestingBase : RealmUnitTestingBase<RealmTestingServer, RealmTestingPlayer>
{
    protected override RealmTestingServer CreateServer(TestConfigurationProvider? configuration = null, Action<ServerBuilder>? configureBuilder = null, Action<ServiceCollection>? configureServices = null)
    {
        _server ??= new RealmTestingServer(configuration ?? new TestConfigurationProvider(""), configureBuilder, services =>
        {
            services.AddRealmTestingServices(false);
            configureServices?.Invoke(services);
        });

        _server.Instantiate<CollisionShapeBehaviour>();

        return _server;
    }

}