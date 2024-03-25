namespace RealmCore.TestingTools;

public abstract class RealmIntegrationTestingBase<TRealmTestingServer, TRealmPlayer> : RealmTestingBase<TRealmTestingServer, TRealmPlayer>
    where TRealmTestingServer : RealmTestingServer<TRealmPlayer>
    where TRealmPlayer : Player
{
    protected abstract string GetConnectionString();

    protected async Task<TRealmTestingServer> CreateServerAsync(Action<ServerBuilder>? configureBuilder = null, Action<ServiceCollection>? configureServices = null)
    {
        if (_server == null)
        {
            _server = CreateServer(new TestConfigurationProvider(GetConnectionString()), configureBuilder, configureServices);
            await _server.GetRequiredService<IDb>().MigrateAsync();
        }
        return _server;
    }

    protected virtual async Task<TRealmPlayer> CreatePlayerAsync(bool signedIn = true, string baseName = "TestPlayer")
    {

        if (_server == null)
            throw new Exception("Server not created.");

        var name = $"{baseName}{Guid.NewGuid()}";
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

    protected override TRealmPlayer CreatePlayer(string baseName = "TestPlayer")
    {
        if (_server == null)
            throw new Exception("Server not created.");

        var name = $"{baseName}{Guid.NewGuid()}";
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
}
