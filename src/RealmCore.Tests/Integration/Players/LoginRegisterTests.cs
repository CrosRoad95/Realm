namespace RealmCore.Tests.Integration.Players;

public class LoginRegisterTests
{
    [Fact]
    public async Task SavingAndLoadingPlayerShouldWork()
    {
        using var _ = new AssertionScope();
        var name = $"foo{Guid.NewGuid()}";

        using var hosting = new RealmTestingServerHosting();

        var now = hosting.DateTimeProvider.Now;
        {
            var player = await hosting.CreatePlayer(name: name, dontLoadData: false);
            player.Spawn(new Vector3(1, 2, 3));

            player.PlayTime.InternalSetTotalPlayTime(1337);
            player.Money.Amount = 12.345m;
            player.DailyVisits.VisitsInRow.Should().Be(1);
            player.Settings.Set(1, "test");
            player.Bans.Add(1, reason: "test");
            player.Upgrades.TryAdd(1);
            player.Statistics.Increase(1, 1);
            player.Statistics.Increase(2, 2);

            await hosting.DisconnectPlayer(player);
        }

        hosting.DateTimeProvider.Add(TimeSpan.FromDays(1));

        void assert(RealmTestingServerHosting server, RealmPlayer player)
        {
            player.Position.Should().Be(new Vector3(1, 2, 3));
            player.PlayTime.TotalPlayTime.Should().Be(TimeSpan.FromSeconds(1337));
            player.Money.Amount.Should().Be(12.345m);
            player.DailyVisits.VisitsInRow.Should().Be(2);
            player.DailyVisits.LastVisit.Date.Should().Be(server.DateTimeProvider.Now.Date);
            player.Settings.TryGet(1, out var setting).Should().BeTrue();
            setting.Should().Be("test");
            player.Bans.IsBanned(1).Should().BeTrue();
            player.Upgrades.Has(1).Should().BeTrue();
            player.Statistics.Should().BeEquivalentTo([new UserStatDto(1, 1), new UserStatDto(2, 2)]);
        }

        for (int i = 0; i < 2; i++)
        {
            var player = await hosting.CreatePlayer(name: name, dontLoadData: false);
            player.TrySpawnAtLastPosition().Should().BeTrue();

            assert(hosting, player);

            await hosting.DisconnectPlayer(player);
        }
    }

    [Fact]
    public async Task RemovingItemShouldRemoveItFromDatabase()
    {
        using var hosting = new RealmTestingServerHosting();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        itemsCollection.Add(1, new ItemsCollectionItem
        {
            Size = 1,
            StackSize = 8,
            AvailableActions = ItemAction.Use,
        });
        itemsCollection.Add(2, new ItemsCollectionItem
        {
            Size = 1,
            StackSize = 8,
            AvailableActions = ItemAction.Use,
        });

        var playerName = $"FakeInvPlayer{Guid.NewGuid()}";
        var player1 = await hosting.CreatePlayer(name: playerName);

        if (!player1.Inventory.TryGetPrimary(out var inventory1))
        {
            inventory1 = player1.Inventory.CreatePrimary(20);
        }
        inventory1.Number.Should().Be(0);
        using(var access = inventory1.Open())
        {
            access.TryAddItem(1);
            access.TryAddItem(1);
            access.TryAddItem(1);
        }
        inventory1.Number.Should().Be(3);

        await hosting.DisconnectPlayer(player1);

        var player2 = await hosting.CreatePlayer(name: playerName, dontLoadData: false);
        var inventory2 = player2.Inventory.Primary!;
        using (var access = inventory2.Open())
        {
            var item = access.Items.First();
            access.RemoveItem(item).Should().BeTrue();
        }
        player2.Inventory.Primary!.Number.Should().Be(2);
        await hosting.DisconnectPlayer(player2);

        var player3 = await hosting.CreatePlayer();
        var inventory3 = player2.Inventory.Primary!;
        player2.Inventory.Primary!.Number.Should().Be(2);
    }
}
