namespace RealmCore.TestingTools;

public abstract class RealmTestingBase<TRealmTestingServer, TRealmPlayer>
    where TRealmTestingServer : RealmTestingServer<TRealmPlayer>
    where TRealmPlayer : Player
{
    protected TRealmTestingServer? _server;

    protected abstract TRealmTestingServer CreateServer(TestConfigurationProvider? cnofiguration = null, Action<ServerBuilder>? configureBuilder = null, Action<IServiceCollection>? configureServices = null);
    protected abstract TRealmPlayer CreatePlayer(string name = "TestPlayer");
}
