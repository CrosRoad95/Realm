﻿namespace RealmCore.TestingTools;

public abstract class RealmIntegrationTestingBase : IAsyncLifetime
{
    protected abstract string DatabaseName { get; }

    private MySqlContainer? _mySqlContainer;
    private RealmTestingServer? _server;
    private MySqlContainer _MySqlContainer
    {
        get
        {
            if (_mySqlContainer == null)
                _mySqlContainer = new MySqlBuilder().WithDatabase(DatabaseName).Build();
            return _mySqlContainer;
        }
    }

    protected async Task<RealmTestingServer> CreateServerAsync(Action<ServiceCollection>? configureServices = null)
    {
        if (_server == null)
        {
            _server = new RealmTestingServer(new TestConfigurationProvider(_MySqlContainer.GetConnectionString()), configureServices);
            await _server.GetRequiredService<IDb>().MigrateAsync();
        }
        return _server;
    }

    protected async Task<RealmPlayer> CreatePlayerAsync(bool signedIn = true)
    {
        if (_server == null)
            throw new Exception("Server not created.");
        var player = _server.CreatePlayer();
        if (signedIn)
        {
            await _server.SignInPlayer(player);
            player.PersistentId.Should().NotBe(0);
        }
        return player;
    }

    protected async Task<RealmVehicle> CreateVehicleAsync()
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