using Microsoft.Extensions.Options;

namespace RealmCore.TestingTools;

public class RealmTestingServerHostingFixture : IDisposable
{
    private readonly RealmTestingServerHosting<RealmTestingPlayer> _hosting;

    public RealmTestingServerHosting<RealmTestingPlayer> Hosting => _hosting;

    public RealmTestingServerHostingFixture()
    {
        _hosting = new RealmTestingServerHosting<RealmTestingPlayer>();
    }

    public void Dispose()
    {
        _hosting.Dispose();
    }
}

public class RealmTestingServerHostingFixtureWithPlayer<TPlayer> : IAsyncLifetime where TPlayer: RealmPlayer
{
    private readonly RealmTestingServerHosting<TPlayer> _hosting;
    private TPlayer? _player;

    public RealmTestingServerHosting<TPlayer> Hosting => _hosting;
    public TPlayer Player => _player ?? throw new InvalidOperationException();

    public RealmTestingServerHostingFixtureWithPlayer()
    {
        _hosting = new RealmTestingServerHosting<TPlayer>();
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

    public virtual void CleanPlayer(RealmPlayer player)
    {
        var gameplayOptions = _hosting.GetRequiredService<IOptions<GameplayOptions>>().Value;
        player.Money.Amount = 0;

        player.Money.SetLimitAndPrecision(gameplayOptions.MoneyLimit, gameplayOptions.MoneyPrecision);
    }
}

public class RealmTestingServerHostingFixtureWithPlayer : RealmTestingServerHostingFixtureWithPlayer<RealmTestingPlayer>
{

}