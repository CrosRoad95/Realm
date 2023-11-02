namespace RealmCore.Tests.Tests.Components;

public class InventoryComponentTests
{
    private void PopulateItemsRegistry(ItemsRegistry itemsRegistry)
    {
        itemsRegistry.Add(1, new ItemRegistryEntry
        {
            Name = "test item id 1",
            Size = 1,
            StackSize = 8,
            AvailableActions = ItemAction.Use,
        });
        itemsRegistry.Add(2, new ItemRegistryEntry
        {
            Name = "test item id 2",
            Size = 2,
            StackSize = 1,
            AvailableActions = ItemAction.Use,
        });
        itemsRegistry.Add(3, new ItemRegistryEntry
        {
            Name = "test item id 3",
            Size = 1,
            StackSize = 8,
            AvailableActions = ItemAction.Use | ItemAction.Drop | ItemAction.Eat,
        });
        itemsRegistry.Add(4, new ItemRegistryEntry
        {
            Name = "test item id 4",
            Size = 100,
            StackSize = 8,
            AvailableActions = ItemAction.Use,
        });
        itemsRegistry.Add(5, new ItemRegistryEntry
        {
            Name = "test item id 5",
            Size = 101,
            StackSize = 8,
            AvailableActions = ItemAction.Use,
        });
        itemsRegistry.Add(6, new ItemRegistryEntry
        {
            Name = "test item id 6",
            Size = 1,
            StackSize = 1,
            AvailableActions = ItemAction.Use,
        });
    }

    [InlineData(1, 1, 1)]
    [InlineData(2, 4, 8)]
    [Theory]
    public void AddItemShouldWork(uint itemId, uint number, uint expectedNumber)
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);

        var inventoryComponent = player.AddComponent(new InventoryComponent(100));
        inventoryComponent.AddItem(itemsRegistry, itemId, number);

        inventoryComponent.Number.Should().Be(expectedNumber);
    }

    [Fact]
    public void ItemsShouldBeAppropriatelyStackedWhenAddedOneStack()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);

        var inventoryComponent = player.AddComponent(new InventoryComponent(100));
        inventoryComponent.AddItem(itemsRegistry, 3, 2, null, true);
        inventoryComponent.AddItem(itemsRegistry, 3, 3, null, true);
        inventoryComponent.AddItem(itemsRegistry, 3, 2, null, true);
        inventoryComponent.AddItem(itemsRegistry, 3, 1, null, true);

        inventoryComponent.Items.Should().HaveCount(1);
        inventoryComponent.Items[0].Number.Should().Be(8);
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void ItemsShouldBeAppropriatelyStackedWhenAddedMultipleStacks(bool tryStack)
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);
        var inventoryComponent = player.AddComponent(new InventoryComponent(100));

        inventoryComponent.AddItem(itemsRegistry, 3, 2, null, tryStack);
        inventoryComponent.AddItem(itemsRegistry, 3, 22, null, tryStack);

        if (tryStack)
        {
            inventoryComponent.Items.Should().HaveCount(3); // 8 + 8 + 8
        }
        else
        {
            inventoryComponent.Items.Should().HaveCount(4); // 2 + 8 + 8 + 6
        }
        inventoryComponent.Items.Sum(x => x.Number).Should().Be(24);
    }

    [Fact]
    public void ItemsShouldNotBeStackedWhenHaveDifferentMetadata()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);
        var inventoryComponent = player.AddComponent(new InventoryComponent(100));

        var metaData = new Metadata
        {
            ["foo"] = 1,
        };

        inventoryComponent.AddItem(itemsRegistry, 3, 2, null);
        inventoryComponent.AddItem(itemsRegistry, 3, 2, null);
        inventoryComponent.AddItem(itemsRegistry, 3, 2, metaData);
        inventoryComponent.AddItem(itemsRegistry, 3, 2, metaData);

        inventoryComponent.Items.Should().HaveCount(2);
        inventoryComponent.Items.Sum(x => x.Number).Should().Be(8);
    }

    [Fact]
    public void HasItemByIdShouldWork()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);
        var inventoryComponent = player.AddComponent(new InventoryComponent(100));

        inventoryComponent.AddItem(itemsRegistry, 1, 1);

        inventoryComponent.HasItemById(1).Should().BeTrue();
    }
    
    [Fact]
    public void SumItemsByIdShouldWork()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);
        var inventoryComponent = player.AddComponent(new InventoryComponent(100));

        inventoryComponent.AddItem(itemsRegistry, 2, 1);
        inventoryComponent.AddItem(itemsRegistry, 2, 1);
        inventoryComponent.AddItem(itemsRegistry, 6, 1);

        inventoryComponent.SumItemsById(2).Should().Be(2);
    }
    
    [Fact]
    public void SumItemsNumberByIdShouldWork()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);
        var inventoryComponent = player.AddComponent(new InventoryComponent(100));

        inventoryComponent.AddItem(itemsRegistry, 2, 4);
        inventoryComponent.AddItem(itemsRegistry, 2, 3);
        inventoryComponent.AddItem(itemsRegistry, 6, 5);

        inventoryComponent.SumItemsById(2).Should().Be(7);
    }
    
    [Fact]
    public void GetItemsByIdShouldWork()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);
        var inventoryComponent = player.AddComponent(new InventoryComponent(100));

        var item1 = inventoryComponent.AddItem(itemsRegistry, 2, 1).First();
        var item2 = inventoryComponent.AddItem(itemsRegistry, 2, 1).First();
        inventoryComponent.AddItem(itemsRegistry, 6, 1);

        inventoryComponent.GetItemsById(2).Should().BeEquivalentTo(new List<Item>
        {
            item1, item2
        });
    }

    [Fact]
    public void GetItemsByIdWithMetadataShouldWork()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);
        var inventoryComponent = player.AddComponent(new InventoryComponent(100));

        var item = inventoryComponent.AddItem(itemsRegistry, 1, 1, new Metadata { ["foo"] = 1 }).First();
        inventoryComponent.AddItem(itemsRegistry, 1, 1, new Metadata { ["foo"] = 2 });
        inventoryComponent.AddItem(itemsRegistry, 1, 1, new Metadata { ["foo"] = 3 });

        inventoryComponent.GetItemsByIdWithMetadata(1, "foo", 1).Should().BeEquivalentTo(new List<Item>
        {
            item
        });
    }

    [Fact]
    public void GetItemsByIdWithMetadataAsDictionaryShouldWork()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);
        var inventoryComponent = player.AddComponent(new InventoryComponent(100));

        var item = inventoryComponent.AddSingleItem(itemsRegistry, 1, new Metadata { ["foo"] = 1 });

        inventoryComponent.GetSingleItemByIdWithMetadata(1, new Metadata
        {
            ["foo"] = 1
        }).Should().BeEquivalentTo(item);
    }

    [Fact]
    public void HasItemWithMetadataShouldWork()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);
        var inventoryComponent = player.AddComponent(new InventoryComponent(100));

        inventoryComponent.AddSingleItem(itemsRegistry, 1, new Metadata { ["foo"] = 1 });
        inventoryComponent.AddItem(itemsRegistry, 1, 1, new Metadata { ["foo"] = 2 });
        inventoryComponent.AddItem(itemsRegistry, 1, 1, new Metadata { ["foo"] = 3 });

        inventoryComponent.HasItemWithMetadata(1, "foo", 1).Should().BeTrue();
        inventoryComponent.HasItemWithMetadata(1, "foo", 4).Should().BeFalse();
    }
    
    [Fact]
    public void YouShouldBeAbleToAddAndGetItemByMetadata()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);
        var inventoryComponent = player.AddComponent(new InventoryComponent(100));

        var item = inventoryComponent.AddSingleItem(itemsRegistry, 1, new Metadata { ["foo"] = 1 });
        var found = inventoryComponent.TryGetByIdAndMetadata(1, new Metadata { ["foo"] = 1 }, out Item foundItem);

        found.Should().BeTrue();
        item.Should().Be(foundItem);
    }

    [Fact]
    public void YouShouldNotBeAbleToAddItemWithLessThanOneNumber()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);
        var inventoryComponent = player.AddComponent(new InventoryComponent(100));

        var act = () => inventoryComponent.AddItem(itemsRegistry, 1, 0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void InventoryShouldBeResizable()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);
        var inventoryComponent = player.AddComponent(new InventoryComponent(100));

        inventoryComponent.HasSpace(200).Should().BeFalse();
        inventoryComponent.Size = 500;
        inventoryComponent.HasSpace(200).Should().BeTrue();
    }

    [Fact]
    public void YouShouldBeAbleToCheckIfItemWillFitIntoInventory()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);
        var inventoryComponent = player.AddComponent(new InventoryComponent(100));

        inventoryComponent.Size = 100;
        inventoryComponent.HasSpaceForItem(1, itemsRegistry).Should().BeTrue();
        inventoryComponent.HasSpaceForItem(4, itemsRegistry).Should().BeTrue();
        inventoryComponent.HasSpaceForItem(5, itemsRegistry).Should().BeFalse();
        inventoryComponent.HasSpaceForItem(1, 100, itemsRegistry).Should().BeTrue();
        inventoryComponent.HasSpaceForItem(1, 101, itemsRegistry).Should().BeFalse();
    }

    private void HandleItemUsed(InventoryComponent inventoryComponent, Item usedItem, ItemAction flags)
    {
        usedItem.SetMetadata("counter", usedItem.GetMetadata<int>("counter") - 1);
    }

    [Fact]
    public void YouShouldBeAbleToUseItem()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);
        var inventoryComponent = player.AddComponent(new InventoryComponent(100));

        inventoryComponent.ItemUsed += HandleItemUsed;
        inventoryComponent.Size = 100;
        var item = inventoryComponent.AddSingleItem(itemsRegistry, 1, new Metadata { ["counter"] = 10 });

        inventoryComponent.TryUseItem(item, ItemAction.Use).Should().BeTrue();
        item.GetMetadata("counter").Should().Be(9);
    }

    [Fact]
    public void YouCanNotUseItemIfItDoesNotSupportIt()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);
        var inventoryComponent = player.AddComponent(new InventoryComponent(100));

        inventoryComponent.Size = 100;
        var item = inventoryComponent.AddSingleItem(itemsRegistry, 1);
        inventoryComponent.TryUseItem(item, ItemAction.Close).Should().BeFalse();
    }

    [Fact]
    public void RemoveItemShouldWork()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);
        var inventoryComponent = player.AddComponent(new InventoryComponent(100));

        var item = inventoryComponent.AddSingleItem(itemsRegistry, 1);
        inventoryComponent.RemoveItem(item);
        inventoryComponent.Items.Should().BeEmpty();
    }
    
    [Fact]
    public void RemoveItemRemoveMultipleStacks()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);
        var inventoryComponent = player.AddComponent(new InventoryComponent(100));

        inventoryComponent.AddItem(itemsRegistry, 1, 20);
        inventoryComponent.RemoveItem(1, 20);
        inventoryComponent.Number.Should().Be(0);
        inventoryComponent.Items.Should().BeEmpty();
    }

    [Fact]
    public void RemoveItemByIdShouldWork()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);
        var inventoryComponent = player.AddComponent(new InventoryComponent(100));

        inventoryComponent.AddSingleItem(itemsRegistry, 1);
        inventoryComponent.RemoveItem(1);
        inventoryComponent.Items.Should().BeEmpty();
    }

    [Fact]
    public void RemoveItemByIdShouldNotRemoveAnyStackIfThereIsNotEnough()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);
        var inventoryComponent = player.AddComponent(new InventoryComponent(100));

        inventoryComponent.AddItem(itemsRegistry, 1, 20);
        inventoryComponent.RemoveItem(30);
        inventoryComponent.Number.Should().Be(20);
    }

    [Fact]
    public void RemoveItemShouldRemoveSingleItemWork()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);
        var inventoryComponent = player.AddComponent(new InventoryComponent(100));

        inventoryComponent.Size = 100;
        inventoryComponent.AddItem(itemsRegistry, 1, 4);
        inventoryComponent.RemoveItem(1);

        var items = inventoryComponent.Items;
        items.Should().HaveCount(1);
        items[0].Number.Should().Be(3);
    }

    [Fact]
    public void RemoveItemStackShouldRemoveEntireStack()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);
        var inventoryComponent = player.AddComponent(new InventoryComponent(100));

        inventoryComponent.Size = 100;
        inventoryComponent.AddItem(itemsRegistry, 1, 4);
        inventoryComponent.RemoveItemStack(1);

        inventoryComponent.Items.Should().BeEmpty();
    }

    [Fact]
    public void AddItemShouldThrowAppropriateException()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);
        var inventoryComponent = player.AddComponent(new InventoryComponent(100));

        inventoryComponent.Size = 5;
        var act = () => inventoryComponent.AddItem(itemsRegistry, 1, 4);

        act.Should().NotThrow();
        act.Should().Throw<InventoryNotEnoughSpaceException>().Where(x => x.InventorySize == 5 && x.RequiredSpace == 8);
    }
    
    [Fact]
    public void Clear()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);
        var inventoryComponent = player.AddComponent(new InventoryComponent(100));

        #region Arrange
        inventoryComponent.AddItem(itemsRegistry, 1, 2);
        inventoryComponent.AddItem(itemsRegistry, 2, 3);
        inventoryComponent.AddItem(itemsRegistry, 3, 4);
        #endregion

        #region Act
        inventoryComponent.Clear();
        #endregion

        #region Assert
        inventoryComponent.Number.Should().Be(0);
        inventoryComponent.Items.Should().BeEmpty();
        #endregion
    }

    [InlineData(1, true, 9, 1)]
    [InlineData(10, true, 0, 10)]
    [InlineData(11, false, 10, 0)]
    [Theory]
    public void TransferItemTest(uint numberOfItemsToTransfer, bool success, int expectedNumberOfItemsInSourceInventory, int expectedNumberOfItemsInDestinationInventory)
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);
        var inventoryComponent = player.AddComponent(new InventoryComponent(100));

        #region Arrange
        inventoryComponent.Clear();
        var destinationInventory = new InventoryComponent(10);
        var metaData = new Metadata
        {
            ["foo"] = 1
        };
        inventoryComponent.AddItem(itemsRegistry, 1, 10, metaData);
        #endregion

        #region Act
        var isSuccess = inventoryComponent.TransferItem(destinationInventory, itemsRegistry, 1, numberOfItemsToTransfer, false);
        #endregion

        #region Assert
        isSuccess.Should().Be(success);
        inventoryComponent.Number.Should().Be(expectedNumberOfItemsInSourceInventory);
        destinationInventory.Number.Should().Be(expectedNumberOfItemsInDestinationInventory);
        destinationInventory.Items.Select(x => x.MetaData).Should().AllBeEquivalentTo(metaData);
        #endregion
    }

    [Fact]
    public void TryGetMetaDataShouldWork()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);
        var inventoryComponent = player.AddComponent(new InventoryComponent(100));

        var item = inventoryComponent.AddSingleItem(itemsRegistry, 1, new Metadata
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
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);
        var inventoryComponent = player.AddComponent(new InventoryComponent(100));

        inventoryComponent.AddSingleItem(itemsRegistry, 1);
        inventoryComponent.HasItem(x => x.ItemId == itemId).Should().Be(has);
    }

    [Fact]
    public void ItShouldBePossibleToCreateNewInventoryComponentWithItems()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);

        var inventoryComponent = player.AddComponent(new InventoryComponent(10, 0, new List<Item>
        {
            new Item(itemsRegistry, 1, 1)
        }));
        inventoryComponent.Number.Should().Be(1);
    }

    [Fact]
    public void ItemMetaDataPropertyShouldReturnCopyOfMetaData()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);
        var inventoryComponent = player.AddComponent(new InventoryComponent(100));

        var metaData = new Metadata
        {
            ["foo"] = 1
        };

        var item = inventoryComponent.AddSingleItem(itemsRegistry, 1, metaData);

        item.MetaData.Should().BeEquivalentTo(metaData);
    }

    [Fact]
    public void ItemMetaDataKeysPropertyShouldReturnCopyOfMetaDataKeys()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);
        var inventoryComponent = player.AddComponent(new InventoryComponent(100));

        var metaData = new Metadata
        {
            ["foo"] = 1
        };

        var item = inventoryComponent.AddSingleItem(itemsRegistry, 1, metaData);

        item.MetaDataKeys.Should().BeEquivalentTo(new List<string> { "foo" });
    }

    [Fact]
    public void YouShouldBeAbleToRemoveMetaData()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);
        var inventoryComponent = player.AddComponent(new InventoryComponent(100));

        var metaData = new Metadata
        {
            ["foo"] = 1
        };

        var item = inventoryComponent.AddSingleItem(itemsRegistry, 1, metaData);
        using var monitoredItem = item.Monitor();

        item.RemoveMetadata("foo");

        monitoredItem.Should().Raise(nameof(Item.MetadataRemoved));
        item.MetaData.Should().BeEmpty();
    }

    [Fact]
    public void YouShouldBeAbleToChangeMetaData()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);
        var inventoryComponent = player.AddComponent(new InventoryComponent(100));

        var metaData = new Metadata
        {
            ["foo"] = 1
        };

        var item = inventoryComponent.AddSingleItem(itemsRegistry, 1, metaData);
        using var monitoredItem = item.Monitor();

        item.ChangeMetadata<int>("foo", x => x + 1);
        ((int?)item.GetMetadata("foo")).Should().Be(2);

        monitoredItem.Should().Raise(nameof(Item.MetadataChanged));
        item.MetaData.Should().BeEquivalentTo(new Metadata
        {
            ["foo"] = 2
        });
    }

    [Fact]
    public void HasMetadataShouldWork()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);
        var inventoryComponent = player.AddComponent(new InventoryComponent(100));

        var metaData = new Metadata
        {
            ["foo"] = 1
        };

        var item = inventoryComponent.AddSingleItem(itemsRegistry, 1, metaData);

        item.HasMetadata("foo").Should().BeTrue();
    }

    [Fact]
    public async Task TransferItemShouldBeThreadSafe()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);
        var inventoryComponent = player.AddComponent(new InventoryComponent(100));

        #region Arrange
        inventoryComponent.Clear();
        var destinationInventory = new InventoryComponent(800);
        inventoryComponent.AddItem(itemsRegistry, 1, 800, null, true, true);
        #endregion

        #region Act
        await ParallelHelpers.Run(() =>
        {
            var isSuccess = inventoryComponent.TransferItem(destinationInventory, itemsRegistry, 1, 1, false);
        });
        #endregion

        #region Assert
        inventoryComponent.Number.Should().Be(0);
        destinationInventory.IsFull.Should().BeTrue();
        #endregion
    }

    [Fact]
    public void RemoveAndGetItemByIdShouldWork()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);
        var inventoryComponent = player.AddComponent(new InventoryComponent(100));

        var metaData = new Metadata
        {
            ["foo"] = 1
        };

        var addedItem = inventoryComponent.AddSingleItem(itemsRegistry, 1, metaData);
        var removedItem = inventoryComponent.RemoveAndGetItemById(1).First();

        addedItem.Should().BeEquivalentTo(removedItem);
    }
    
    [Fact]
    public void RemoveAndGetItemByIdShouldWorkForManyItems()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);
        var inventoryComponent = player.AddComponent(new InventoryComponent(100));

        var metaData = new Metadata
        {
            ["foo"] = 1
        };

        var addedItem = inventoryComponent.AddItem(itemsRegistry, 1, 20, metaData);
        var removedItem = inventoryComponent.RemoveAndGetItemById(1, 20);

        addedItem.Should().BeEquivalentTo(removedItem);
    }
    
    [Fact]
    public void RemovingOneItemAndGetItemByIdShouldWorkForManyItems()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var itemsRegistry = realmTestingServer.GetRequiredService<ItemsRegistry>();
        PopulateItemsRegistry(itemsRegistry);
        var inventoryComponent = player.AddComponent(new InventoryComponent(100));

        var metaData = new Metadata
        {
            ["foo"] = 1
        };

        inventoryComponent.AddItem(itemsRegistry, 1, 20, metaData);
        var removedItem = inventoryComponent.RemoveAndGetItemById(1, 1);
        inventoryComponent.Number.Should().Be(19);
        removedItem.Should().HaveCount(1);
        removedItem.First().MetaData.Should().BeEquivalentTo(metaData);
    }
}
