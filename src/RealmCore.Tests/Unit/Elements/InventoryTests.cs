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
            access.Items.Should().HaveCount(2);
        }

        inventory.Items.Should().HaveCount(2);
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
