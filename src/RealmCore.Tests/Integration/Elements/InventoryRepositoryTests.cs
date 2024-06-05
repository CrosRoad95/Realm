namespace RealmCore.Tests.Integration.Elements;

public class InventoryRepositoryTests : IClassFixture<RealmTestingServerHostingFixture>
{
    private readonly RealmTestingServerHostingFixture _fixture;

    public InventoryRepositoryTests(RealmTestingServerHostingFixture realmTestingServerHostingFixture)
    {
        _fixture = realmTestingServerHostingFixture;
    }

    [Fact]
    public async Task InventoryRepositoryShouldWork()
    {
        var player = await _fixture.Hosting.CreatePlayer();

        var repository = _fixture.Hosting.GetRequiredService<IInventoryRepository>();
        var id = player.User.Id;
         
        var inventoryId = await repository.CreateInventoryForUserId(id, 22);
        var inventory = await repository.GetInventoryById(inventoryId);

        if (inventory == null)
            throw new Exception("Inventory should not be null");
        inventory.Size.Should().Be(22);
        inventory.InventoryItems.Should().BeEmpty();
    }

    [Fact]
    public async Task AddingInventoryViaRepositoryShouldWork()
    {
        var player = await _fixture.Hosting.CreatePlayer(name: Guid.NewGuid().ToString());

        var repository = _fixture.Hosting.GetRequiredService<IInventoryRepository>();
        var id = player.User.Id;

        await _fixture.Hosting.LogOutPlayer(player);
        await repository.CreateInventoryForUserId(id, 24);
        await _fixture.Hosting.LoginPlayer(player, false);

        player.Inventory.Primary!.Size.Should().Be(24);
    }
}
