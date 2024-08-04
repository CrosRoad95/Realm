namespace RealmCore.Tests.Integration;

public class TransactionContextTests
{
    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task TransactionContextShouldWork(bool shouldThrow)
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer(name: Guid.NewGuid().ToString());

        var repository = hosting.GetRequiredService<InventoryRepository>();
        var transactionContext = hosting.GetRequiredService<ITransactionContext>();
        var id = player.User.Id;

        var act = async () =>
        {
            await transactionContext.ExecuteAsync(async (db) =>
            {
                var inventoryId = await repository.CreateInventoryForUserId(id, 22);
                if(shouldThrow)
                    throw new Exception("nope");
            });
        };

        if (shouldThrow)
            await act.Should().ThrowAsync<Exception>().WithMessage("nope");
        else
            await act.Should().NotThrowAsync();

        var inventories = await repository.GetAllInventoriesByUserId(id);

        if (shouldThrow)
        {
            inventories.Should().BeEmpty();
        }
        else
        {
            inventories.Should().HaveCount(1);
        }
    }
}
