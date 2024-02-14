namespace RealmCore.Tests;

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

    protected async Task<RealmTestingServer> CreateServerAsync()
    {
        if(_server == null)
        {
            _server = new RealmTestingServer(new TestConfigurationProvider(_MySqlContainer.GetConnectionString()));
            await _server.GetRequiredService<IDb>().MigrateAsync();
        }
        return _server;
    }

    protected async Task<RealmPlayer> CreatePlayerAsync()
    {
        if (_server == null)
            throw new Exception("Server not created.");
        var player = await _server.SignInPlayer(_server.CreatePlayer());
        player.PersistentId.Should().Be(1);
        return player;
    }

    public Task InitializeAsync() => _MySqlContainer.StartAsync();

    public Task DisposeAsync() => _MySqlContainer.DisposeAsync().AsTask();
}
