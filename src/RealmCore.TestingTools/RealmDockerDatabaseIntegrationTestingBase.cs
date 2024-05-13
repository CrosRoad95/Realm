namespace RealmCore.TestingTools;

public abstract class RealmDockerDatabaseIntegrationTestingBase<TRealmTestingServer, TRealmPlayer> : RealmIntegrationTestingBase<TRealmTestingServer, TRealmPlayer>, IAsyncLifetime
    where TRealmTestingServer : RealmTestingServer<TRealmPlayer>
    where TRealmPlayer : Player
{
    protected abstract string DatabaseName { get; }

    private MySqlContainer? _mySqlContainer;
    private MySqlContainer _MySqlContainer
    {
        get
        {
            if (_mySqlContainer == null)
                _mySqlContainer = new MySqlBuilder().WithDatabase(DatabaseName).Build();
            return _mySqlContainer;
        }
    }

    public Task InitializeAsync() => _MySqlContainer.StartAsync();

    protected override string GetConnectionString()
    {
        return _MySqlContainer.GetConnectionString();
    }

    public Task DisposeAsync() => _MySqlContainer.DisposeAsync().AsTask();
}

public abstract class RealmDockerDatabaseIntegrationTestingBase : RealmDockerDatabaseIntegrationTestingBase<RealmTestingServer, RealmTestingPlayer>
{
    protected override RealmTestingServer CreateServer(TestConfigurationProvider? cnofiguration = null, Action<ServerBuilder>? configureBuilder = null, Action<IServiceCollection>? configureServices = null)
    {
        _server ??= new RealmTestingServer(cnofiguration ?? new TestConfigurationProvider(""), configureBuilder, services =>
        {
            services.AddRealmTestingServices(true);
            configureServices?.Invoke(services);
        });
        return _server;
    }
}