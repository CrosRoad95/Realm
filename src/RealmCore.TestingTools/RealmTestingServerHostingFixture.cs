
namespace RealmCore.TestingTools;

public class RealmTestingServerHostingFixture : IDisposable
{
    private readonly RealmTestingServerHosting _hosting;

    public RealmTestingServerHosting Hosting => _hosting;

    public RealmTestingServerHostingFixture()
    {
        _hosting = new RealmTestingServerHosting();
    }

    public void Dispose()
    {
        _hosting.Dispose();
    }
}

public class RealmTestingServerHostingFixtureWithPlayer : IAsyncLifetime
{
    private readonly RealmTestingServerHosting _hosting;
    private RealmTestingPlayer? _player;

    public RealmTestingServerHosting Hosting => _hosting;
    public RealmTestingPlayer Player => _player ?? throw new InvalidOperationException();

    public RealmTestingServerHostingFixtureWithPlayer()
    {
        _hosting = new RealmTestingServerHosting();
    }

    public async Task InitializeAsync()
    {
        _player = await _hosting.CreatePlayer();
    }

    public Task DisposeAsync()
    {

        _hosting.Dispose();
        return Task.CompletedTask;
    }
}
