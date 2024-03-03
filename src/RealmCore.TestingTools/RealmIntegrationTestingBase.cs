namespace RealmCore.TestingTools;

public abstract class RealmIntegrationTestingBase<TRealmTestingServer, TRealmPlayer> : RealmTestingBase<TRealmTestingServer, TRealmPlayer>, IAsyncLifetime
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

    protected async Task<TRealmTestingServer> CreateServerAsync(Action<ServerBuilder>? configureBuilder = null, Action<ServiceCollection>? configureServices = null)
    {
        if (_server == null)
        {
            _server = CreateServer(new TestConfigurationProvider(_MySqlContainer.GetConnectionString()), configureBuilder, configureServices);
            await _server.GetRequiredService<IDb>().MigrateAsync();
        }
        return _server;
    }

    protected virtual async Task<TRealmPlayer> CreatePlayerAsync(bool signedIn = true, string name = "TestPlayer")
    {
        if (_server == null)
            throw new Exception("Server not created.");
        var player = CreatePlayer(name);
        if (signedIn)
        {
            if(player is RealmPlayer realmPlayer)
            {
                await _server.SignInPlayer(realmPlayer);
                realmPlayer.PersistentId.Should().NotBe(0);
            }
        }
        return player;
    }

    protected override TRealmPlayer CreatePlayer(string name = "TestPlayer")
    {
        if (_server == null)
            throw new Exception("Server not created.");

        var player = _server.CreatePlayer(name: name);
        if (player.IsDestroyed)
            return player;

        return player;
    }

    protected virtual async Task<RealmVehicle> CreateVehicleAsync()
    {
        if (_server == null)
            throw new Exception("Server not created.");
        var vehiclesService = _server.GetRequiredService<IVehiclesService>();
        var vehicle = await vehiclesService.CreatePersistantVehicle(Location.Zero, (VehicleModel)404);
        vehicle.PersistentId.Should().NotBe(0);
        return vehicle;
    }

    public Task InitializeAsync() => _MySqlContainer.StartAsync();

    public Task DisposeAsync() => _MySqlContainer.DisposeAsync().AsTask();
}

public abstract class RealmIntegrationTestingBase : RealmIntegrationTestingBase<RealmTestingServer, RealmTestingPlayer>
{
    protected override RealmTestingServer CreateServer(TestConfigurationProvider? cnofiguration = null, Action<ServerBuilder>? configureBuilder = null, Action<ServiceCollection>? configureServices = null)
    {
        _server ??= new RealmTestingServer(cnofiguration ?? new TestConfigurationProvider(""), configureBuilder, services =>
        {
            services.AddRealmTestingServices(true);
            configureServices?.Invoke(services);
        });
        return _server;
    }

}