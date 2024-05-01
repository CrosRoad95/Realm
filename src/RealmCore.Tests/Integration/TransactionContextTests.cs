namespace RealmCore.Tests.Integration;

[Collection("IntegrationTests")]
public class TransactionContextTests : RealmRemoteDatabaseIntegrationTestingBase
{
    protected override string DatabaseName => "TransactionContextTests";

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task TransactionContextShouldWork(bool shouldThrow)
    {
        var server = await CreateServerAsync();
        var player = await CreatePlayerAsync();

        var repository = server.GetRequiredService<IInventoryRepository>();
        var transactionContext = server.GetRequiredService<ITransactionContext>();
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
