namespace RealmCore.Tests.Tests.Components;

public class InventoryComponentTests
{
    private readonly ItemsRegistry _itemsRegistry;
    private readonly Entity _entity;
    private readonly InventoryComponent _inventoryComponent;

    public InventoryComponentTests()
    {
        _itemsRegistry = new();
        _itemsRegistry.Add(1, new ItemRegistryEntry
        {
            Name = "test item id 1",
            Size = 1,
            StackSize = 8,
            AvailableActions = ItemAction.Use,
        });
        _itemsRegistry.Add(2, new ItemRegistryEntry
        {
            Name = "test item id 2",
            Size = 2,
            StackSize = 1,
            AvailableActions = ItemAction.Use,
        });
        _itemsRegistry.Add(3, new ItemRegistryEntry
        {
            Name = "test item id 3",
            Size = 1,
            StackSize = 8,
            AvailableActions = ItemAction.Use | ItemAction.Drop | ItemAction.Eat,
        });
        _itemsRegistry.Add(4, new ItemRegistryEntry
        {
            Name = "test item id 4",
            Size = 100,
            StackSize = 8,
            AvailableActions = ItemAction.Use,
        });
        _itemsRegistry.Add(5, new ItemRegistryEntry
        {
            Name = "test item id 5",
            Size = 101,
            StackSize = 8,
            AvailableActions = ItemAction.Use,
        });
        _itemsRegistry.Add(6, new ItemRegistryEntry
        {
            Name = "test item id 6",
            Size = 1,
            StackSize = 1,
            AvailableActions = ItemAction.Use,
        });

        var services = new ServiceCollection();
        services.AddSingleton<IRealmConfigurationProvider>(new TestConfigurationProvider());
        _entity = new(services.BuildServiceProvider(), "test");
        _inventoryComponent = new(100);
        _entity.AddComponent(_inventoryComponent);
    }

    [InlineData(1, 1, 1)]
    [InlineData(2, 4, 8)]
    [Theory]
    public void AddItemShouldWork(uint itemId, uint number, uint expectedNumber)
    {
        _inventoryComponent.AddItem(_itemsRegistry, itemId, number);

        _inventoryComponent.Number.Should().Be(expectedNumber);
    }

    [Fact]
    public void ItemsShouldBeAppropriatelyStackedWhenAddedOneStack()
    {
        _inventoryComponent.AddItem(_itemsRegistry, 3, 2, null, true);
        _inventoryComponent.AddItem(_itemsRegistry, 3, 3, null, true);
        _inventoryComponent.AddItem(_itemsRegistry, 3, 2, null, true);
        _inventoryComponent.AddItem(_itemsRegistry, 3, 1, null, true);

        _inventoryComponent.Items.Should().HaveCount(1);
        _inventoryComponent.Items.First().Number.Should().Be(8);
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void ItemsShouldBeAppropriatelyStackedWhenAddedMultipleStacks(bool tryStack)
    {
        _inventoryComponent.AddItem(_itemsRegistry, 3, 2, null, tryStack);
        _inventoryComponent.AddItem(_itemsRegistry, 3, 22, null, tryStack);

        if (tryStack)
        {
            _inventoryComponent.Items.Should().HaveCount(3); // 8 + 8 + 8
        }
        else
        {
            _inventoryComponent.Items.Should().HaveCount(4); // 2 + 8 + 8 + 6
        }
        _inventoryComponent.Items.Sum(x => x.Number).Should().Be(24);
    }

    [Fact]
    public void ItemsShouldNotBeStackedWhenHaveDifferentMetadata()
    {
        var metaData = new Dictionary<string, object>
        {
            ["foo"] = 1,
        };

        _inventoryComponent.AddItem(_itemsRegistry, 3, 2, null);
        _inventoryComponent.AddItem(_itemsRegistry, 3, 2, null);
        _inventoryComponent.AddItem(_itemsRegistry, 3, 2, metaData);
        _inventoryComponent.AddItem(_itemsRegistry, 3, 2, metaData);

        _inventoryComponent.Items.Should().HaveCount(2);
        _inventoryComponent.Items.Sum(x => x.Number).Should().Be(8);
    }

    [Fact]
    public void HasItemByIdShouldWork()
    {
        _inventoryComponent.AddItem(_itemsRegistry, 1, 1);

        _inventoryComponent.HasItemById(1).Should().BeTrue();
    }
    
    [Fact]
    public void SumItemsByIdShouldWork()
    {
        _inventoryComponent.AddItem(_itemsRegistry, 2, 1);
        _inventoryComponent.AddItem(_itemsRegistry, 2, 1);
        _inventoryComponent.AddItem(_itemsRegistry, 6, 1);

        _inventoryComponent.SumItemsById(2).Should().Be(2);
    }
    
    [Fact]
    public void SumItemsNumberByIdShouldWork()
    {
        _inventoryComponent.AddItem(_itemsRegistry, 2, 4);
        _inventoryComponent.AddItem(_itemsRegistry, 2, 3);
        _inventoryComponent.AddItem(_itemsRegistry, 6, 5);

        _inventoryComponent.SumItemsById(2).Should().Be(7);
    }
    
    [Fact]
    public void GetItemsByIdShouldWork()
    {
        var item1 = _inventoryComponent.AddItem(_itemsRegistry, 2, 1).First();
        var item2 = _inventoryComponent.AddItem(_itemsRegistry, 2, 1).First();
        _inventoryComponent.AddItem(_itemsRegistry, 6, 1);

        _inventoryComponent.GetItemsById(2).Should().BeEquivalentTo(new List<Item>
        {
            item1, item2
        });
    }

    [Fact]
    public void GetItemsByIdWithMetadataShouldWork()
    {
        var item = _inventoryComponent.AddItem(_itemsRegistry, 1, 1, new Dictionary<string, object> { ["foo"] = 1 }).First();
        _inventoryComponent.AddItem(_itemsRegistry, 1, 1, new Dictionary<string, object> { ["foo"] = 2 });
        _inventoryComponent.AddItem(_itemsRegistry, 1, 1, new Dictionary<string, object> { ["foo"] = 3 });

        _inventoryComponent.GetItemsByIdWithMetadata(1, "foo", 1).Should().BeEquivalentTo(new List<Item>
        {
            item
        });
    }

    [Fact]
    public void GetItemsByIdWithMetadataAsDictionaryShouldWork()
    {
        var item = _inventoryComponent.AddSingleItem(_itemsRegistry, 1, new Dictionary<string, object> { ["foo"] = 1 });

        _inventoryComponent.GetSingleItemByIdWithMetadata(1, new Dictionary<string, object>
        {
            ["foo"] = 1
        }).Should().BeEquivalentTo(item);
    }

    [Fact]
    public void HasItemWithMetadataShouldWork()
    {
        var item = _inventoryComponent.AddSingleItem(_itemsRegistry, 1, new Dictionary<string, object> { ["foo"] = 1 });
        _inventoryComponent.AddItem(_itemsRegistry, 1, 1, new Dictionary<string, object> { ["foo"] = 2 });
        _inventoryComponent.AddItem(_itemsRegistry, 1, 1, new Dictionary<string, object> { ["foo"] = 3 });

        _inventoryComponent.HasItemWithMetadata(1, "foo", 1).Should().BeTrue();
        _inventoryComponent.HasItemWithMetadata(1, "foo", 4).Should().BeFalse();
    }
    
    [Fact]
    public void YouShouldBeAbleToAddAndGetItemByMetadata()
    {
        var item = _inventoryComponent.AddSingleItem(_itemsRegistry, 1, new Dictionary<string, object> { ["foo"] = 1 });
        var found = _inventoryComponent.TryGetByIdAndMetadata(1, new Dictionary<string, object> { ["foo"] = 1 }, out Item foundItem);

        found.Should().BeTrue();
        item.Should().Be(foundItem);
    }

    [Fact]
    public void YouShouldNotBeAbleToAddItemWithLessThanOneNumber()
    {
        var act = () => _inventoryComponent.AddItem(_itemsRegistry, 1, 0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void InventoryShouldBeResizable()
    {
        _inventoryComponent.HasSpace(200).Should().BeFalse();
        _inventoryComponent.Size = 500;
        _inventoryComponent.HasSpace(200).Should().BeTrue();
    }

    [Fact]
    public void YouShouldBeAbleToCheckIfItemWillFitIntoInventory()
    {
        _inventoryComponent.Size = 100;
        _inventoryComponent.HasSpaceForItem(1, _itemsRegistry).Should().BeTrue();
        _inventoryComponent.HasSpaceForItem(4, _itemsRegistry).Should().BeTrue();
        _inventoryComponent.HasSpaceForItem(5, _itemsRegistry).Should().BeFalse();
        _inventoryComponent.HasSpaceForItem(1, 100, _itemsRegistry).Should().BeTrue();
        _inventoryComponent.HasSpaceForItem(1, 101, _itemsRegistry).Should().BeFalse();
    }

    [Fact]
    public void YouShouldBeAbleToUseItem()
    {
        void HandleItemUsed(InventoryComponent inventoryComponent, Item usedItem, ItemAction flags)
        {
            usedItem.SetMetadata("counter", usedItem.GetMetadata<int>("counter") - 1);
        }

        _inventoryComponent.ItemUsed += HandleItemUsed;
        _inventoryComponent.Size = 100;
        var item = _inventoryComponent.AddSingleItem(_itemsRegistry, 1, new Dictionary<string, object> { ["counter"] = 10 });

        _inventoryComponent.TryUseItem(item, ItemAction.Use).Should().BeTrue();
        item.GetMetadata("counter").Should().Be(9);
    }

    [Fact]
    public void YouCanNotUseItemIfItDoesNotSupportIt()
    {
        _inventoryComponent.Size = 100;
        var item = _inventoryComponent.AddSingleItem(_itemsRegistry, 1);
        _inventoryComponent.TryUseItem(item, ItemAction.Close).Should().BeFalse();
    }

    [Fact]
    public void RemoveItemShouldWork()
    {
        var item = _inventoryComponent.AddSingleItem(_itemsRegistry, 1);
        _inventoryComponent.RemoveItem(item);
        _inventoryComponent.Items.Should().BeEmpty();
    }
    
    [Fact]
    public void RemoveItemRemoveMultipleStacks()
    {
        _inventoryComponent.AddItem(_itemsRegistry, 1, 20);
        _inventoryComponent.RemoveItem(1, 20);
        _inventoryComponent.Number.Should().Be(0);
        _inventoryComponent.Items.Should().BeEmpty();
    }

    [Fact]
    public void RemoveItemByIdShouldWork()
    {
        var item = _inventoryComponent.AddSingleItem(_itemsRegistry, 1);
        _inventoryComponent.RemoveItem(1);
        _inventoryComponent.Items.Should().BeEmpty();
    }

    [Fact]
    public void RemoveItemByIdShouldNotRemoveAnyStackIfThereIsNotEnough()
    {
        var item = _inventoryComponent.AddItem(_itemsRegistry, 1, 20);
        _inventoryComponent.RemoveItem(30);
        _inventoryComponent.Number.Should().Be(20);
    }

    [Fact]
    public void RemoveItemShouldRemoveSingleItemWork()
    {
        _inventoryComponent.Size = 100;
        _inventoryComponent.AddItem(_itemsRegistry, 1, 4);
        _inventoryComponent.RemoveItem(1);

        var items = _inventoryComponent.Items;
        items.Should().HaveCount(1);
        items.First().Number.Should().Be(3);
    }

    [Fact]
    public void RemoveItemStackShouldRemoveEntireStack()
    {
        _inventoryComponent.Size = 100;
        _inventoryComponent.AddItem(_itemsRegistry, 1, 4);
        _inventoryComponent.RemoveItemStack(1);

        _inventoryComponent.Items.Should().BeEmpty();
    }

    [Fact]
    public void AddItemShouldThrowAppropriateException()
    {
        _inventoryComponent.Size = 5;
        var act = () => _inventoryComponent.AddItem(_itemsRegistry, 1, 4);

        act.Should().NotThrow();
        act.Should().Throw<InventoryNotEnoughSpaceException>().Where(x => x.InventorySize == 5 && x.RequiredSpace == 8);
    }
    
    [Fact]
    public void Clear()
    {
        #region Arrange
        _inventoryComponent.AddItem(_itemsRegistry, 1, 2);
        _inventoryComponent.AddItem(_itemsRegistry, 2, 3);
        _inventoryComponent.AddItem(_itemsRegistry, 3, 4);
        #endregion

        #region Act
        _inventoryComponent.Clear();
        #endregion

        #region Assert
        _inventoryComponent.Number.Should().Be(0);
        _inventoryComponent.Items.Should().BeEmpty();
        #endregion
    }

    [InlineData(1, true, 9, 1)]
    [InlineData(10, true, 0, 10)]
    [InlineData(11, false, 10, 0)]
    [Theory]
    public void TransferItemTest(uint numberOfItemsToTransfer, bool success, int expectedNumberOfItemsInSourceInventory, int expectedNumberOfItemsInDestinationInventory)
    {
        #region Arrange
        _inventoryComponent.Clear();
        var destinationInventory = new InventoryComponent(10);
        var metaData = new Dictionary<string, object>
        {
            ["foo"] = 1
        };
        _inventoryComponent.AddItem(_itemsRegistry, 1, 10, metaData);
        #endregion

        #region Act
        var isSuccess = _inventoryComponent.TransferItem(destinationInventory, _itemsRegistry, 1, numberOfItemsToTransfer, false);
        #endregion

        #region Assert
        isSuccess.Should().Be(success);
        _inventoryComponent.Number.Should().Be(expectedNumberOfItemsInSourceInventory);
        destinationInventory.Number.Should().Be(expectedNumberOfItemsInDestinationInventory);
        destinationInventory.Items.Select(x => x.MetaData).Should().AllBeEquivalentTo(metaData);
        #endregion
    }

    [Fact]
    public void TryGetMetaDataShouldWork()
    {
        var item = _inventoryComponent.AddSingleItem(_itemsRegistry, 1, new Dictionary<string, object>
        {
            ["number"] = 123,
            ["string"] = "123",
            ["object"] = new int[] { 1, 2, 3 }
        });

        item.TryGetMetadata("number", out int numberValue).Should().BeTrue();
        numberValue.Should().Be(123);
        item.TryGetMetadata("string", out string stringValue).Should().BeTrue();
        stringValue.Should().Be("123");
        item.TryGetMetadata("object", out int[] objectValue).Should().BeTrue();
        objectValue.Should().BeEquivalentTo(new int[] { 1, 2, 3 });
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(2, false)]
    public void TestHasItemWithCallback(uint itemId, bool has)
    {
        _inventoryComponent.AddSingleItem(_itemsRegistry, 1);
        _inventoryComponent.HasItem(x => x.ItemId == itemId).Should().Be(has);
    }

    [Fact]
    public void ItShouldBePossibleToCreateNewInventoryComponentWithItems()
    {
        var inventoryComponent = new InventoryComponent(10, 0, new List<Item>
        {
            new Item(_itemsRegistry, 1, 1)
        });
        inventoryComponent.Number.Should().Be(1);
    }

    [Fact]
    public void ItemMetaDataPropertyShouldReturnCopyOfMetaData()
    {
        var metaData = new Dictionary<string, object>
        {
            ["foo"] = 1
        };

        var item = _inventoryComponent.AddSingleItem(_itemsRegistry, 1, metaData);

        item.MetaData.Should().BeEquivalentTo(metaData);
    }

    [Fact]
    public void ItemMetaDataKeysPropertyShouldReturnCopyOfMetaDataKeys()
    {
        var metaData = new Dictionary<string, object>
        {
            ["foo"] = 1
        };

        var item = _inventoryComponent.AddSingleItem(_itemsRegistry, 1, metaData);

        item.MetaDataKeys.Should().BeEquivalentTo(new List<string> { "foo" });
    }

    [Fact]
    public void YouShouldBeAbleToRemoveMetaData()
    {
        var metaData = new Dictionary<string, object>
        {
            ["foo"] = 1
        };

        var item = _inventoryComponent.AddSingleItem(_itemsRegistry, 1, metaData);
        using var monitoredItem = item.Monitor();

        item.RemoveMetadata("foo");

        monitoredItem.Should().Raise(nameof(Item.MetadataRemoved));
        item.MetaData.Should().BeEmpty();
    }

    [Fact]
    public void YouShouldBeAbleToChangeMetaData()
    {
        var metaData = new Dictionary<string, object>
        {
            ["foo"] = 1
        };

        var item = _inventoryComponent.AddSingleItem(_itemsRegistry, 1, metaData);
        using var monitoredItem = item.Monitor();

        item.ChangeMetadata<int>("foo", x => x + 1);
        ((int?)item.GetMetadata("foo")).Should().Be(2);

        monitoredItem.Should().Raise(nameof(Item.MetadataChanged));
        item.MetaData.Should().BeEquivalentTo(new Dictionary<string, object>
        {
            ["foo"] = 2
        });
    }

    [Fact]
    public void HasMetadataShouldWork()
    {
        var metaData = new Dictionary<string, object>
        {
            ["foo"] = 1
        };

        var item = _inventoryComponent.AddSingleItem(_itemsRegistry, 1, metaData);

        item.HasMetadata("foo").Should().BeTrue();
    }

    [Fact]
    public async Task TransferItemShouldBeThreadSafe()
    {
        #region Arrange
        _inventoryComponent.Clear();
        var destinationInventory = new InventoryComponent(800);
        _inventoryComponent.AddItem(_itemsRegistry, 1, 800, null, true, true);
        #endregion

        #region Act
        await ParallelHelpers.Run(() =>
        {
            var isSuccess = _inventoryComponent.TransferItem(destinationInventory, _itemsRegistry, 1, 1, false);
        });
        #endregion

        #region Assert
        _inventoryComponent.Number.Should().Be(0);
        destinationInventory.IsFull.Should().BeTrue();
        #endregion
    }

    [Fact]
    public void RemoveAndGetItemByIdShouldWork()
    {
        var metaData = new Dictionary<string, object>
        {
            ["foo"] = 1
        };

        var addedItem = _inventoryComponent.AddSingleItem(_itemsRegistry, 1, metaData);
        var removedItem = _inventoryComponent.RemoveAndGetItemById(1).First();

        addedItem.Should().BeEquivalentTo(removedItem);
    }
    
    [Fact]
    public void RemoveAndGetItemByIdShouldWorkForManyItems()
    {
        var metaData = new Dictionary<string, object>
        {
            ["foo"] = 1
        };

        var addedItem = _inventoryComponent.AddItem(_itemsRegistry, 1, 20, metaData);
        var removedItem = _inventoryComponent.RemoveAndGetItemById(1, 20);

        addedItem.Should().BeEquivalentTo(removedItem);
    }
    
    [Fact]
    public void RemovingOneItemAndGetItemByIdShouldWorkForManyItems()
    {
        var metaData = new Dictionary<string, object>
        {
            ["foo"] = 1
        };

        _inventoryComponent.AddItem(_itemsRegistry, 1, 20, metaData);
        var removedItem = _inventoryComponent.RemoveAndGetItemById(1, 1);
        _inventoryComponent.Number.Should().Be(19);
        removedItem.Should().HaveCount(1);
        removedItem.First().MetaData.Should().BeEquivalentTo(metaData);
    }
}
