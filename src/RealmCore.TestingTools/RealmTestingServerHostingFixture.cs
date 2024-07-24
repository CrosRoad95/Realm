using Microsoft.Extensions.Options;
using RealmCore.Server.Modules.Inventories;

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
        _player.Inventory.CreatePrimary(100);
        var itemsCollection = _hosting.GetRequiredService<ItemsCollection>();
        Seed(itemsCollection);
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

    private void Seed(ItemsCollection itemsCollection)
    {
        itemsCollection.Add(1, new ItemsCollectionItem
        {
            Size = 1,
            StackSize = 8,
            AvailableActions = ItemAction.Use,
        });
        itemsCollection.Add(2, new ItemsCollectionItem
        {
            Size = 2,
            StackSize = 1,
            AvailableActions = ItemAction.Use,
        });
        itemsCollection.Add(3, new ItemsCollectionItem
        {
            Size = 1,
            StackSize = 8,
            AvailableActions = ItemAction.Use | ItemAction.Drop | ItemAction.Eat,
        });
        itemsCollection.Add(4, new ItemsCollectionItem
        {
            Size = 100,
            StackSize = 8,
            AvailableActions = ItemAction.Use,
        });
        itemsCollection.Add(5, new ItemsCollectionItem
        {
            Size = 101,
            StackSize = 8,
            AvailableActions = ItemAction.Use,
        });
        itemsCollection.Add(6, new ItemsCollectionItem
        {
            Size = 1,
            StackSize = 1,
            AvailableActions = ItemAction.Use,
        });
    }

}

public class RealmTestingServerHostingFixtureWithPlayer : RealmTestingServerHostingFixtureWithPlayer<RealmTestingPlayer>
{

}