namespace RealmCore.Tests.Unit.Elements;

public class InventoryTests : IClassFixture<RealmTestingServerHostingFixtureWithPlayer>, IDisposable
{
    private readonly RealmTestingServerHostingFixtureWithPlayer _fixture;
    private readonly RealmTestingPlayer _player;
    private readonly Inventory _inventory;
    private readonly ItemsCollection _itemsCollection;

    public InventoryTests(RealmTestingServerHostingFixtureWithPlayer fixture)
    {
        _fixture = fixture;
        _player = _fixture.Player;
        _inventory = _player.Inventory.Primary;
        _itemsCollection = _fixture.Hosting.GetRequiredService<ItemsCollection>();
    }

    [InlineData(1, 1, 1, 1, new string[] {"ItemAdded" })]
    [InlineData(1, 2, 2, 1, new string[] { "ItemAdded" })]
    [InlineData(2, 2, 4, 2, new string[] { "ItemAdded", "ItemAdded" })]
    [Theory]
    public void AddItemShouldWork(uint itemId, uint number, uint expectedNumber, int expectedNumberOfItems, string[] expectedEvents)
    {
        using var monitor = _inventory.Monitor();

        _inventory.AddItems(itemId, number);
        _inventory.Number.Should().Be(expectedNumber);
        _inventory.Items.Should().HaveCount(expectedNumberOfItems);
        monitor.GetOccurredEvents().Should().BeEquivalentTo(expectedEvents);
    }

    [Fact]
    public void ItemsShouldBeAppropriatelyStackedWhenAddedOneStack()
    {
        using var monitor = _inventory.Monitor();

        _inventory.AddItems(3, 2, null, true);
        _inventory.AddItems(3, 3, null, true);
        _inventory.AddItems(3, 2, null, true);
        _inventory.AddItems(3, 1, null, true);

        _inventory.Items.Should().HaveCount(1);
        _inventory.Items[0].Number.Should().Be(8);

        monitor.GetOccurredEvents().Should().BeEquivalentTo(["ItemAdded", "ItemChanged", "ItemChanged", "ItemChanged"]);
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void ItemsShouldBeAppropriatelyStackedWhenAddedMultipleStacks(bool tryStack)
    {
        using var monitor = _inventory.Monitor();

        _inventory.AddItems(3, 2, null, tryStack);
        _inventory.AddItems(3, 22, null, tryStack);

        if (tryStack)
        {
            _inventory.Items.Should().HaveCount(3); // 8 + 8 + 8
        }
        else
        {
            _inventory.Items.Should().HaveCount(4); // 2 + 8 + 8 + 6
        }
        _inventory.Items.Sum(x => x.Number).Should().Be(24);

        if (tryStack)
        {
            monitor.GetOccurredEvents().Should().BeEquivalentTo(["ItemAdded", "ItemChanged", "ItemAdded", "ItemAdded"]);
        }
        else
        {
            monitor.GetOccurredEvents().Should().BeEquivalentTo(["ItemAdded", "ItemAdded", "ItemAdded", "ItemAdded"]);
        }
    }

    [Fact]
    public void ItemsShouldNotBeStackedWhenHaveDifferentMetadata()
    {
        var metaData = new ItemMetadata
        {
            ["foo"] = 1,
        };

        _inventory.AddItems(3, 2, null);
        _inventory.AddItems(3, 2, null);
        _inventory.AddItems(3, 2, metaData);
        _inventory.AddItems(3, 2, metaData);

        _inventory.Items.Should().HaveCount(2);
        _inventory.Items.Sum(x => x.Number).Should().Be(8);
    }

    [Fact]
    public void HasItemByIdShouldWork()
    {
        _inventory.AddItems(1, 1);

        _inventory.HasItemById(1).Should().BeTrue();
    }

    [Fact]
    public void SumItemsByIdShouldWork()
    {
        _inventory.AddItems(2, 1);
        _inventory.AddItems(2, 1);
        _inventory.AddItems(6, 1);

        _inventory.SumItemsById(2).Should().Be(2);
    }

    [Fact]
    public void SumItemsNumberByIdShouldWork()
    {
        _inventory.AddItems(2, 4);
        _inventory.AddItems(2, 3);
        _inventory.AddItems(6, 5);

        _inventory.SumItemsById(2).Should().Be(7);
    }

    [Fact]
    public void GetItemsByIdShouldWork()
    {
        var item1 = _inventory.AddItems(2, 1).First();
        var item2 = _inventory.AddItems(2, 1).First();
        _inventory.AddItems(6, 1);

        _inventory.GetItemsById(2).Should().BeEquivalentTo(new List<InventoryItem>
        {
            item1, item2
        });
    }

    [Fact]
    public void GetItemsByIdWithMetadataShouldWork()
    {
        var item = _inventory.AddItems(1, 1, new ItemMetadata { ["foo"] = 1 }).First();
        _inventory.AddItems(1, 1, new ItemMetadata { ["foo"] = 2 });
        _inventory.AddItems(1, 1, new ItemMetadata { ["foo"] = 3 });

        _inventory.GetItemsByIdWithMetadata(1, "foo", 1).Should().BeEquivalentTo(new List<InventoryItem>
        {
            item
        });
    }

    [Fact]
    public void GetItemsByIdWithMetadataAsDictionaryShouldWork()
    {
        var item = _inventory.AddItem(1, new ItemMetadata { ["foo"] = 1 });

        _inventory.GetSingleItemByIdWithMetadata(1, new ItemMetadata
        {
            ["foo"] = 1
        }).Should().BeEquivalentTo(item);
    }

    [Fact]
    public void HasItemWithMetadataShouldWork()
    {
        _inventory.AddItem(1, new ItemMetadata { ["foo"] = 1 });
        _inventory.AddItems(1, 1, new ItemMetadata { ["foo"] = 2 });
        _inventory.AddItems(1, 1, new ItemMetadata { ["foo"] = 3 });

        _inventory.HasItemWithMetadata(1, "foo", 1).Should().BeTrue();
        _inventory.HasItemWithMetadata(1, "foo", 4).Should().BeFalse();
    }

    [Fact]
    public void YouShouldBeAbleToAddAndGetItemByMetadata()
    {
        var item = _inventory.AddItem(1, new ItemMetadata { ["foo"] = 1 });
        var found = _inventory.TryGetByIdAndMetadata(1, new ItemMetadata { ["foo"] = 1 }, out InventoryItem foundItem);

        found.Should().BeTrue();
        item.Should().Be(foundItem);
    }

    [Fact]
    public void YouShouldNotBeAbleToAddItemWithLessThanOneNumber()
    {
        var act = () => _inventory.AddItems(1, 0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void InventoryShouldBeResizable()
    {
        _inventory.HasSpace(200).Should().BeFalse();
        _inventory.Size = 500;
        _inventory.HasSpace(200).Should().BeTrue();
    }

    [Fact]
    public void YouShouldBeAbleToCheckIfItemWillFitIntoInventory()
    {
        _inventory.HasSpaceForItem(1).Should().BeTrue();
        _inventory.HasSpaceForItem(4).Should().BeTrue();
        _inventory.HasSpaceForItem(5).Should().BeFalse();
        _inventory.HasSpaceForItem(1, 100).Should().BeTrue();
        _inventory.HasSpaceForItem(1, 101).Should().BeFalse();
    }

    [Fact]
    public void HasSpaceForEdgeCase()
    {
        _inventory.Size = 10;
        _inventory.HasSpaceForItem(1, 10).Should().BeTrue();
        _inventory.HasSpaceForItem(1, 11).Should().BeFalse();
    }

    [Fact]
    public void YouShouldBeAbleToUseItem()
    {
        void handleItemUsed(Inventory _inventory, InventoryItem usedItem, ItemAction flags)
        {
            usedItem.SetMetadata("counter", usedItem.GetMetadata<int>("counter") - 1);
        }

        _inventory.ItemUsed += handleItemUsed;
        var item = _inventory.AddItem(1, new ItemMetadata { ["counter"] = 10 });

        _inventory.TryUseItem(item, ItemAction.Use).Should().BeTrue();
        item.GetMetadata("counter").Should().Be(9);
    }

    [Fact]
    public void YouCanNotUseItemIfItDoesNotSupportIt()
    {
        _inventory.Size = 100;
        var item = _inventory.AddItem(1);
        _inventory.TryUseItem(item, ItemAction.Close).Should().BeFalse();
    }

    [Fact]
    public void RemoveItemShouldWork()
    {
        var item = _inventory.AddItem(1);
        _inventory.RemoveItem(item.LocalId);
        _inventory.Items.Should().BeEmpty();
    }

    [Fact]
    public void RemoveItemRemoveMultipleStacks()
    {
        var items = _inventory.AddItems(1, 20);
        foreach (var item in items)
        {
            _inventory.RemoveItem(item.LocalId, item.Number);
        }
        _inventory.Number.Should().Be(0);
        _inventory.Items.Should().BeEmpty();
    }

    [Fact]
    public void RemoveItemShouldRemoveSingleItemWork()
    {
        var createdItems = _inventory.AddItems(1, 4);
        _inventory.RemoveItem(createdItems.First().LocalId);

        var items = _inventory.Items;
        items.Should().HaveCount(1);
        items[0].Number.Should().Be(3);
    }

    [Fact]
    public void RemoveItemStackShouldRemoveEntireStack()
    {
        var items = _inventory.AddItems(1, 4);
        _inventory.RemoveItem(items.First().LocalId, 4);

        _inventory.Items.Should().BeEmpty();
    }

    [Fact]
    public void AddItemShouldThrowAppropriateException()
    {
        _inventory.Size = 5;
        var act = () => _inventory.AddItems(1, 4);

        act.Should().NotThrow();
        act.Should().Throw<InventoryNotEnoughSpaceException>();
    }

    [Fact]
    public void ClearShouldWork()
    {
        #region Arrange
        _inventory.AddItems(1, 2);
        _inventory.AddItems(2, 3);
        _inventory.AddItems(3, 4);
        #endregion

        #region Act
        _inventory.Clear();
        #endregion

        #region Assert
        _inventory.Number.Should().Be(0);
        _inventory.Items.Should().BeEmpty();
        #endregion
    }

    [Fact]
    public void SimpleTransferItemTest1()
    {
        var destinationInventory = new Inventory(_player, 8, _itemsCollection);

        _inventory.AddItems(1, 8, new ItemMetadata
        {
            ["foo"] = 1
        });

        _inventory.TryGetByItemId(1, out var inventoryItem);

        using var monitorSource = _inventory.Monitor();
        using var monitorDestination = destinationInventory.Monitor();
        var isSuccess = _inventory.TransferItem(destinationInventory, inventoryItem.LocalId, 8);

        using var _ = new AssertionScope();
        _inventory.Number.Should().Be(0);
        destinationInventory.Number.Should().Be(8);
        monitorSource.GetOccurredEvents().Should().BeEquivalentTo(["ItemRemoved"]);
        monitorDestination.GetOccurredEvents().Should().BeEquivalentTo(["ItemAdded"]);
    }
    
    [Fact]
    public void SimpleTransferItemTest2()
    {
        var destinationInventory = new Inventory(_player, 8, _itemsCollection);

        _inventory.AddItems(1, 8, new ItemMetadata
        {
            ["foo"] = 1
        });

        _inventory.TryGetByItemId(1, out var inventoryItem);

        using var monitorSource = _inventory.Monitor();
        using var monitorDestination = destinationInventory.Monitor();
        var isSuccess = _inventory.TransferItem(destinationInventory, inventoryItem.LocalId, 1);

        using var _ = new AssertionScope();
        _inventory.Number.Should().Be(7);
        destinationInventory.Number.Should().Be(1);
        monitorSource.GetOccurredEvents().Should().BeEquivalentTo(["ItemChanged"]);
        monitorDestination.GetOccurredEvents().Should().BeEquivalentTo(["ItemAdded"]);
    }
    
    [InlineData(0, 1, true, 7, 1, new string[] { "ItemChanged" }, new string[] { "ItemAdded" })]
    [InlineData(0, 8, true, 0, 8, new string[] { "ItemRemoved" }, new string[] { "ItemAdded" })]
    [InlineData(0, 9, false, 8, 0, new string[] { }, new string[] { })]
    [InlineData(1, 1, true, 7, 2, new string[] { "ItemChanged" }, new string[] { "ItemAdded" })]
    [InlineData(1, 7, true, 1, 8, new string[] { "ItemChanged" }, new string[] { "ItemAdded" })]
    [InlineData(1, 8, false, 8, 1, new string[] { }, new string[] { })]
    [Theory]
    public void TransferItemTest1(uint initialNumberOfItems, uint numberOfItemsToTransfer, bool shouldSuccess, int expectedNumberOfItemsInSourceInventory, int expectedNumberOfItemsInDestinationInventory, string[] sourceExpectedEvents, string[] destinationExpectedEvents)
    {
        var destinationInventory = new Inventory(_player, 8, _itemsCollection);

        var metaData = new ItemMetadata
        {
            ["foo"] = 1
        };
        _inventory.AddItems(1, 8, metaData);
        if(initialNumberOfItems > 0)
            destinationInventory.AddItems(1, initialNumberOfItems, metaData);

        _inventory.TryGetByItemId(1, out var inventoryItem);

        using var monitorSource = _inventory.Monitor();
        using var monitorDestination = destinationInventory.Monitor();
        var isSuccess = _inventory.TransferItem(destinationInventory, inventoryItem.LocalId, numberOfItemsToTransfer, false);

        using var _ = new AssertionScope();
        isSuccess.Should().Be(shouldSuccess);
        _inventory.Number.Should().Be(expectedNumberOfItemsInSourceInventory);
        destinationInventory.Number.Should().Be(expectedNumberOfItemsInDestinationInventory);
        destinationInventory.Items.Select(x => x.MetaData).Should().AllBeEquivalentTo(metaData);

        monitorSource.GetOccurredEvents().Should().BeEquivalentTo(sourceExpectedEvents);
        monitorDestination.GetOccurredEvents().Should().BeEquivalentTo(destinationExpectedEvents);
    }
    
    [Fact]
    public void TryGetMetaDataShouldWork()
    {
        var item = _inventory.AddItem(1, new ItemMetadata
        {
            ["number"] = 123,
            ["string"] = "123",
            ["object"] = new int[] { 1, 2, 3 }
        });

        item.TryGetMetadata("number", out int numberValue).Should().BeTrue();
        numberValue.Should().Be(123);
        item.TryGetMetadata("string", out string? stringValue).Should().BeTrue();
        stringValue.Should().Be("123");
        item.TryGetMetadata("object", out int[]? objectValue).Should().BeTrue();
        objectValue.Should().BeEquivalentTo(new int[] { 1, 2, 3 });
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(2, false)]
    public void TestHasItemWithCallback(uint itemId, bool has)
    {
        _inventory.AddItem(1);
        _inventory.HasItem(x => x.ItemId == itemId).Should().Be(has);
    }

    [Fact]
    public void ItShouldBePossibleToCreateNewInventoryWithItems()
    {
        var _inventory = new Inventory(_player, 10, 0, [
            new(_itemsCollection, 1, 1)
        ], _itemsCollection);
        _inventory.Number.Should().Be(1);
    }

    [Fact]
    public void ItemMetaDataPropertyShouldReturnCopyOfMetaData()
    {
        var metaData = new ItemMetadata
        {
            ["foo"] = 1
        };

        var item = _inventory.AddItem(1, metaData);

        item.MetaData.Should().BeEquivalentTo(metaData);
    }

    [Fact]
    public void ItemMetaDataKeysPropertyShouldReturnCopyOfMetaDataKeys()
    {
        var metaData = new ItemMetadata
        {
            ["foo"] = 1
        };

        var item = _inventory.AddItem(1, metaData);

        item.MetadataKeys.Should().BeEquivalentTo(new List<string> { "foo" });
    }

    [Fact]
    public void YouShouldBeAbleToRemoveMetaData()
    {
        var metaData = new ItemMetadata
        {
            ["foo"] = 1
        };

        var item = _inventory.AddItem(1, metaData);
        using var monitoredItem = item.Monitor();

        item.RemoveMetadata("foo");

        monitoredItem.Should().Raise(nameof(InventoryItem.MetadataRemoved));
        item.MetaData.Should().BeEmpty();
    }

    [Fact]
    public void YouShouldBeAbleToChangeMetaData()
    {
        var metaData = new ItemMetadata
        {
            ["foo"] = 1
        };

        var item = _inventory.AddItem(1, metaData);
        using var monitoredItem = item.Monitor();

        item.ChangeMetadata<int>("foo", x => x + 1);
        ((int?)item.GetMetadata("foo")).Should().Be(2);

        monitoredItem.Should().Raise(nameof(InventoryItem.MetadataChanged));
        item.MetaData.Should().BeEquivalentTo(new ItemMetadata
        {
            ["foo"] = 2
        });
    }

    [Fact]
    public void HasMetadataShouldWork()
    {
        var metaData = new ItemMetadata
        {
            ["foo"] = 1
        };

        var item = _inventory.AddItem(1, metaData);

        item.HasMetadata("foo").Should().BeTrue();
    }

    [Fact]
    public void YouShouldBeAbleToModifyInventoryInInventoryChangedEventCallback()
    {
        void handleItemAdded(Inventory that, InventoryItem addedItem)
        {
            if (that.Number < 5)
            {
                that.AddItem(1);
            }
        }

        _inventory.ItemAdded += handleItemAdded;
        _inventory.ItemChanged += handleItemAdded;

        _inventory.AddItem(1);
        _inventory.Number.Should().Be(5);
    }

    [Fact]
    public void AddingItemObjectsToInventoryShouldWork()
    {
        _inventory.AddItem(1);
        _inventory.AddItem(1);
        _inventory.AddItem(1);
        _inventory.AddItem(1);

        var items = _inventory.GetItemsById(1);
        items.Should().HaveCount(1);

        items.First().Number.Should().Be(4);
    }

    [Fact]
    public void ItemsOfDifferentMetadataShouldNotStack()
    {
        _inventory.AddItems(1, 4, new ItemMetadata
        {
            ["foo"] = 1
        });
        _inventory.AddItems(1, 4, new ItemMetadata
        {
            ["bar"] = 1
        });

        var items = _inventory.GetItemsById(1);

        using var _ = new AssertionScope();
        items.Should().HaveCount(2);

        items.First().Number.Should().Be(4);
        items.Last().Number.Should().Be(4);
    }

    public void Dispose()
    {
        _inventory.Clear();
        _inventory.Number.Should().Be(0);
        _inventory.Size = 100;
    }
}
