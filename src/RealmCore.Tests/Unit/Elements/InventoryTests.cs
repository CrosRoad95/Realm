namespace RealmCore.Tests.Unit.Elements;

public class InventoryTests : IClassFixture<RealmTestingServerHostingFixtureWithPlayer>, IDisposable
{
    private readonly RealmTestingServerHostingFixtureWithPlayer _fixture;
    private readonly RealmTestingPlayer _player;
    private readonly ElementInventory _inventory;
    private readonly ItemsCollection _itemsCollection;

    public InventoryTests(RealmTestingServerHostingFixtureWithPlayer fixture)
    {
        _fixture = fixture;
        _player = _fixture.Player;
        _inventory = _player.Inventory.Primary!;
        _itemsCollection = _fixture.Hosting.GetRequiredService<ItemsCollection>();
    }

    [Fact]
    public void AddingItemShouldWork()
    {
        var inventory = new Inventory(20, _itemsCollection);
        using var monitor = inventory.Monitor();

        using(var access = inventory.Open())
        {
            access.TryAddItem(1);
        }

        using var _ = new AssertionScope();
        inventory.Items.Should().HaveCount(1);
        monitor.GetOccurredEvents().Should().BeEquivalentTo(["ItemAdded"]);
    }

    [Fact]
    public void AddingAndRemovingItemShouldWork()
    {
        var inventory = new Inventory(20, _itemsCollection);
        using var monitor = inventory.Monitor();

        using(var access = inventory.Open())
        {
            access.TryAddItem(1);
        }
        
        using(var access = inventory.Open())
        {
            access.RemoveItem(access.FindItem(x => x.ItemId == 1)!);
        }

        using var _ = new AssertionScope();
        inventory.Items.Should().HaveCount(0);
        monitor.GetOccurredEvents().Should().BeEquivalentTo(["ItemAdded", "ItemRemoved"]);
    }
    
    [Fact]
    public void ItemsTransactionShouldWork()
    {
        var inventory = new Inventory(20, _itemsCollection);
        using var monitor = inventory.Monitor();

        var wait1 = new AutoResetEvent(false);
        var wait2 = new AutoResetEvent(false);
        Task.Run(() =>
        {
            wait1.WaitOne();
            inventory.Items.Should().BeEmpty();
            wait2.Set();
        });

        using(var access = inventory.Open())
        {
            access.TryAddItem(1);
            wait1.Set();
            access.Items.Should().HaveCount(1);
            wait2.WaitOne();
        }

        using var _ = new AssertionScope();
        inventory.Items.Should().HaveCount(1);
        monitor.GetOccurredEvents().Should().BeEquivalentTo(["ItemAdded"]);
    }
    
    [Fact]
    public void InventorySizeShouldBeRespected()
    {
        var inventory = new Inventory(2, _itemsCollection);
        using var monitor = inventory.Monitor();

        using(var access = inventory.Open())
        {
            access.TryAddItem(1).Should().BeTrue();
            access.TryAddItem(1).Should().BeTrue();
            access.TryAddItem(1).Should().BeFalse();
            access.Items.Should().HaveCount(1);
        }

        inventory.Items.Should().HaveCount(1);
        inventory.Number.Should().Be(2);
    }
    
    [Fact]
    public void OnlyOneThreadCanAccessInventory()
    {
        var inventory = new Inventory(20, _itemsCollection);
        using var monitor = inventory.Monitor();

        inventory.Open();
        var act = () => inventory.Open(TimeSpan.FromMilliseconds(10));

        act.Should().Throw<TimeoutException>();
    }

    [Fact]
    public void YouShouldBeAbleToChangeItemMetadata()
    {
        var inventory = new Inventory(20, _itemsCollection);
        using var monitor = inventory.Monitor();

        using (var access = inventory.Open())
        {
            access.TryAddItem(1, 1, new ItemMetadata
            {
                ["foo"] = 1
            });

            var item = access.Items.Where(x => x.ItemId == 1).Single();
            item.MetaData["foo"].Should().Be(1);
            item.SetMetadata("bar", "baz");
        }

        inventory.Items[0].MetaData["bar"].Should().Be("baz");
    }
    
    [Fact]
    public void YouShouldNotBeAbleToRemoveMoreItemsThanYouHave1()
    {
        var inventory = new Inventory(20, _itemsCollection);

        using (var access = inventory.Open())
        {
            access.TryAddItem(1, 4);
            access.RemoveItem(access.Items.First(), 5).Should().BeFalse();
        }

        inventory.Number.Should().Be(4);
    }
    
    [Fact]
    public void YouShouldNotBeAbleToRemoveMoreItemsThanYouHave2()
    {
        var inventory = new Inventory(20, _itemsCollection);

        using (var access = inventory.Open())
        {
            access.TryAddItem(1, 4);
            access.RemoveItem(access.Items.First(), 3).Should().BeTrue();
            access.RemoveItem(access.Items.First(), 3).Should().BeFalse();
        }

        inventory.Number.Should().Be(1);
    }

    [Fact]
    public void YouShouldNotBeAbleToRemoveMoreItemsThanYouHave3()
    {
        var inventory = new Inventory(20, _itemsCollection);

        using (var access = inventory.Open())
        {
            access.TryAddItem(1, 20);
            access.RemoveItem(access.Items.First(), 8).Should().BeTrue();
            access.RemoveItem(access.Items.First(), 8).Should().BeTrue();
            access.RemoveItem(access.Items.First(), 4).Should().BeTrue();
        }

        inventory.Number.Should().Be(0);
    }

    [Fact]
    public void StackingItemsShouldWork1()
    {
        var inventory = new Inventory(20, _itemsCollection);

        using (var access = inventory.Open())
        {
            access.TryAddItem(1, 4);
            access.TryAddItem(1, 4);
            access.Items.First().Number.Should().Be(8);
            access.Items.Should().HaveCount(1);
        }

        inventory.Number.Should().Be(8);
    }
    
    [Fact]
    public void YouCanNotAddMoreItemsThanInventorySize()
    {
        var inventory = new Inventory(20, _itemsCollection);

        using (var access = inventory.Open())
        {
            access.TryAddItem(1, 21);
            access.Items.Should().BeEmpty();
        }

        inventory.Number.Should().Be(0);
    }

    [InlineData(1, new string[] { "ItemAdded", "ItemAdded", "ItemAdded" })]
    [InlineData(2, new string[] { "ItemAdded", "ItemAdded", "ItemChanged", "ItemAdded" })]
    [InlineData(3, new string[] { "ItemAdded", "ItemAdded", "ItemAdded" })]
    [InlineData(4, new string[] { "ItemAdded", "ItemChanged", "ItemChanged", "ItemChanged", "ItemChanged", "ItemChanged", "ItemChanged", "ItemChanged", "ItemAdded", "ItemChanged", "ItemChanged", "ItemChanged", "ItemChanged", "ItemChanged", "ItemChanged", "ItemChanged", "ItemAdded", "ItemChanged", "ItemChanged", "ItemChanged" })]
    [Theory]
    public void StackingItemsShouldWork2(int variant, string[] expectedEvents)
    {
        var inventory = new Inventory(20, _itemsCollection);

        using var monitor = inventory.Monitor();
        using (var access = inventory.Open())
        {
            switch (variant)
            {
                case 1:
                    access.TryAddItem(1, 20);
                    break;
                case 2:
                    access.TryAddItem(1, 10);
                    access.TryAddItem(1, 10);
                    break;
                case 3:
                    access.TryAddItem(1, 8);
                    access.TryAddItem(1, 8);
                    access.TryAddItem(1, 4);
                    break;
                case 4:
                    {
                        for(int i = 0; i < 20;i++)
                            access.TryAddItem(1, 1);
                    }
                    break;
            }

            var items = access.Items.ToArray();
            items.Should().HaveCount(3);
            items[0].Number.Should().Be(8);
            items[1].Number.Should().Be(8);
            items[2].Number.Should().Be(4);
        }

        monitor.GetOccurredEvents().Should().BeEquivalentTo(expectedEvents);
        inventory.Number.Should().Be(20);
    }

    [Fact]
    public void StackingItemsShouldWork3()
    {
        var inventory = new Inventory(20, _itemsCollection);

        using (var access = inventory.Open())
        {
            access.TryAddItem(1, 4, tryStack: false);
            access.TryAddItem(1, 4, tryStack: false);
            access.Items.First().Number.Should().Be(4);
            access.Items.Last().Number.Should().Be(4);
            access.Items.Should().HaveCount(2);
        }

        inventory.Number.Should().Be(8);
    }

    [Fact]
    public void StackingItemsShouldWork4()
    {
        var inventory = new Inventory(20, _itemsCollection);

        using (var access = inventory.Open())
        {
            access.TryAddItem(1, 1, null);
            access.Items.Should().HaveCount(1);
            access.Items.First().Number.Should().Be(1);
        }
        
        using (var access = inventory.Open())
        {
            access.TryAddItem(1, 1, null);
            access.Items.Should().HaveCount(1);
            access.Items.First().Number.Should().Be(2);
        }

        inventory.Number.Should().Be(2);
    }

    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [Theory]
    public void TransferItemShouldWork(int variant)
    {
        var inventory1 = new Inventory(20, _itemsCollection);
        var inventory2 = new Inventory(20, _itemsCollection);

        using (var access = inventory1.Open())
        {
            access.TryAddItem(1, 20);
        }

        using (var access1 = inventory1.Open())
        {
            using var access2 = inventory2.Open();
            switch (variant)
            {
                case 1:
                    for (int i = 0; i < 20; i++)
                        access1.TransferItem(access2, access1.Items.First().LocalId, 1).Should().BeTrue($"Iteration: {i}");
                    break;
                case 2:
                    for (int i = 0; i < 20; i += 2)
                        access1.TransferItem(access2, access1.Items.First().LocalId, 2).Should().BeTrue($"Iteration: {i}");
                    break;
                case 3:
                    access1.TransferItem(access2, access1.Items.First().LocalId, 8).Should().BeTrue();
                    access1.TransferItem(access2, access1.Items.First().LocalId, 8).Should().BeTrue();
                    access1.TransferItem(access2, access1.Items.First().LocalId, 4).Should().BeTrue();
                    break;
            }
        }

        using var _ = new AssertionScope();
        inventory1.Number.Should().Be(0);
        inventory2.Number.Should().Be(20);
    }

    [Fact]
    public void YouCanNotTransferMoreItemsThanYouHave()
    {
        var inventory1 = new Inventory(20, _itemsCollection);
        var inventory2 = new Inventory(20, _itemsCollection);

        using (var access = inventory1.Open())
        {
            access.TryAddItem(1, 20);
        }

        using (var access1 = inventory1.Open())
        {
            using var access2 = inventory2.Open();
            access1.TransferItem(access2, access1.Items.First().LocalId, 9).Should().BeFalse();
        }

        using var _ = new AssertionScope();
        inventory1.Number.Should().Be(20);
        inventory2.Number.Should().Be(0);
    }

    public void Dispose()
    {
        using (var access = _inventory.Open())
        {
            access.Clear();
        }
        _inventory.Number.Should().Be(0);
        _inventory.Size = 100;
    }
}
