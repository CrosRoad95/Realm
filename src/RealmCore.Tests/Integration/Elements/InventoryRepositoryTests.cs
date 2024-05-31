namespace RealmCore.Tests.Integration.Elements;

public class InventoryRepositoryTests
{
    [Fact]
    public async Task InventoryRepositoryShouldWork()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var repository = hosting.GetRequiredService<IInventoryRepository>();
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
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer(name: Guid.NewGuid().ToString());

        var repository = hosting.GetRequiredService<IInventoryRepository>();
        var id = player.User.Id;

        await hosting.LogOutPlayer(player);
        await repository.CreateInventoryForUserId(id, 24);
        await hosting.LoginPlayer(player, false);

        player.Inventory.Primary!.Size.Should().Be(24);
    }
}
