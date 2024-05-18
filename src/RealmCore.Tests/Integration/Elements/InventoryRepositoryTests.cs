namespace RealmCore.Tests.Integration.Elements;

[Collection("IntegrationTests")]
public class InventoryRepositoryTests : RealmRemoteDatabaseIntegrationTestingBase
{
    [Fact]
    public async Task InventoryRepositoryShouldWork()
    {
        var server = await CreateServerAsync();
        var player = await CreatePlayerAsync();

        var repository = server.GetRequiredService<IInventoryRepository>();
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
        var server = await CreateServerAsync();
        var player = await CreatePlayerAsync();

        var repository = server.GetRequiredService<IInventoryRepository>();
        var id = player.User.Id;

        await server.SignOutPlayer(player);
        await repository.CreateInventoryForUserId(id, 24);
        await server.LoginPlayer(player);

        player.Inventory.Primary!.Size.Should().Be(24);
    }
}
