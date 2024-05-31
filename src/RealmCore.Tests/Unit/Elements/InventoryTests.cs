namespace RealmCore.Tests.Unit.Elements;

public class InventoryTests
{
    private void Seed(RealmTestingServerHosting server)
    {
        var itemsCollection = server.GetRequiredService<ItemsCollection>();
        itemsCollection.Add(1, new ItemsCollectionItem
        {
            Size = 1,
            StackSize = 8,
            AvailableActions = ItemAction.Use,
        });
        itemsCollection.Add(2, new ItemsCollectionItem
        {
            Size = 2,
            StackSize = 1,
            AvailableActions = ItemAction.Use,
        });
        itemsCollection.Add(3, new ItemsCollectionItem
        {
            Size = 1,
            StackSize = 8,
            AvailableActions = ItemAction.Use | ItemAction.Drop | ItemAction.Eat,
        });
        itemsCollection.Add(4, new ItemsCollectionItem
        {
            Size = 100,
            StackSize = 8,
            AvailableActions = ItemAction.Use,
        });
        itemsCollection.Add(5, new ItemsCollectionItem
        {
            Size = 101,
            StackSize = 8,
            AvailableActions = ItemAction.Use,
        });
        itemsCollection.Add(6, new ItemsCollectionItem
        {
            Size = 1,
            StackSize = 1,
            AvailableActions = ItemAction.Use,
        });
    }

    [InlineData(1, 1, 1)]
    [InlineData(2, 4, 8)]
    [Theory]
    public async Task AddItemShouldWork(uint itemId, uint number, uint expectedNumber)
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);

        var inventory = player.Inventory.CreatePrimary(100);
        inventory.AddItem(itemsCollection, itemId, number);

        inventory.Number.Should().Be(expectedNumber);
    }

    [Fact]
    public async Task ItemsShouldBeAppropriatelyStackedWhenAddedOneStack()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);

        var inventory = player.Inventory.CreatePrimary(100);
        inventory.AddItem(itemsCollection, 3, 2, null, true);
        inventory.AddItem(itemsCollection, 3, 3, null, true);
        inventory.AddItem(itemsCollection, 3, 2, null, true);
        inventory.AddItem(itemsCollection, 3, 1, null, true);

        inventory.Items.Should().HaveCount(1);
        inventory.Items[0].Number.Should().Be(8);
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task ItemsShouldBeAppropriatelyStackedWhenAddedMultipleStacks(bool tryStack)
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);
        var inventory = player.Inventory.CreatePrimary(100);

        inventory.AddItem(itemsCollection, 3, 2, null, tryStack);
        inventory.AddItem(itemsCollection, 3, 22, null, tryStack);

        if (tryStack)
        {
            inventory.Items.Should().HaveCount(3); // 8 + 8 + 8
        }
        else
        {
            inventory.Items.Should().HaveCount(4); // 2 + 8 + 8 + 6
        }
        inventory.Items.Sum(x => x.Number).Should().Be(24);
    }

    [Fact]
    public async Task ItemsShouldNotBeStackedWhenHaveDifferentMetadata()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);
        var inventory = player.Inventory.CreatePrimary(100);

        var metaData = new ItemMetadata
        {
            ["foo"] = 1,
        };

        inventory.AddItem(itemsCollection, 3, 2, null);
        inventory.AddItem(itemsCollection, 3, 2, null);
        inventory.AddItem(itemsCollection, 3, 2, metaData);
        inventory.AddItem(itemsCollection, 3, 2, metaData);

        inventory.Items.Should().HaveCount(2);
        inventory.Items.Sum(x => x.Number).Should().Be(8);
    }

    [Fact]
    public async Task HasItemByIdShouldWork()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);
        var inventory = player.Inventory.CreatePrimary(100);

        inventory.AddItem(itemsCollection, 1, 1);

        inventory.HasItemById(1).Should().BeTrue();
    }

    [Fact]
    public async Task SumItemsByIdShouldWork()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);
        var inventory = player.Inventory.CreatePrimary(100);

        inventory.AddItem(itemsCollection, 2, 1);
        inventory.AddItem(itemsCollection, 2, 1);
        inventory.AddItem(itemsCollection, 6, 1);

        inventory.SumItemsById(2).Should().Be(2);
    }

    [Fact]
    public async Task SumItemsNumberByIdShouldWork()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);
        var inventory = player.Inventory.CreatePrimary(100);

        inventory.AddItem(itemsCollection, 2, 4);
        inventory.AddItem(itemsCollection, 2, 3);
        inventory.AddItem(itemsCollection, 6, 5);

        inventory.SumItemsById(2).Should().Be(7);
    }

    [Fact]
    public async Task GetItemsByIdShouldWork()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);
        var inventory = player.Inventory.CreatePrimary(100);

        var item1 = inventory.AddItem(itemsCollection, 2, 1).First();
        var item2 = inventory.AddItem(itemsCollection, 2, 1).First();
        inventory.AddItem(itemsCollection, 6, 1);

        inventory.GetItemsById(2).Should().BeEquivalentTo(new List<InventoryItem>
        {
            item1, item2
        });
    }

    [Fact]
    public async Task GetItemsByIdWithMetadataShouldWork()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);
        var inventory = player.Inventory.CreatePrimary(100);

        var item = inventory.AddItem(itemsCollection, 1, 1, new ItemMetadata { ["foo"] = 1 }).First();
        inventory.AddItem(itemsCollection, 1, 1, new ItemMetadata { ["foo"] = 2 });
        inventory.AddItem(itemsCollection, 1, 1, new ItemMetadata { ["foo"] = 3 });

        inventory.GetItemsByIdWithMetadata(1, "foo", 1).Should().BeEquivalentTo(new List<InventoryItem>
        {
            item
        });
    }

    [Fact]
    public async Task GetItemsByIdWithMetadataAsDictionaryShouldWork()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);
        var inventory = player.Inventory.CreatePrimary(100);

        var item = inventory.AddSingleItem(itemsCollection, 1, new ItemMetadata { ["foo"] = 1 });

        inventory.GetSingleItemByIdWithMetadata(1, new ItemMetadata
        {
            ["foo"] = 1
        }).Should().BeEquivalentTo(item);
    }

    [Fact]
    public async Task HasItemWithMetadataShouldWork()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);
        var inventory = player.Inventory.CreatePrimary(100);

        inventory.AddSingleItem(itemsCollection, 1, new ItemMetadata { ["foo"] = 1 });
        inventory.AddItem(itemsCollection, 1, 1, new ItemMetadata { ["foo"] = 2 });
        inventory.AddItem(itemsCollection, 1, 1, new ItemMetadata { ["foo"] = 3 });

        inventory.HasItemWithMetadata(1, "foo", 1).Should().BeTrue();
        inventory.HasItemWithMetadata(1, "foo", 4).Should().BeFalse();
    }

    [Fact]
    public async Task YouShouldBeAbleToAddAndGetItemByMetadata()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);
        var inventory = player.Inventory.CreatePrimary(100);

        var item = inventory.AddSingleItem(itemsCollection, 1, new ItemMetadata { ["foo"] = 1 });
        var found = inventory.TryGetByIdAndMetadata(1, new ItemMetadata { ["foo"] = 1 }, out InventoryItem foundItem);

        found.Should().BeTrue();
        item.Should().Be(foundItem);
    }

    [Fact]
    public async Task YouShouldNotBeAbleToAddItemWithLessThanOneNumber()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);
        var inventory = player.Inventory.CreatePrimary(100);

        var act = () => inventory.AddItem(itemsCollection, 1, 0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task InventoryShouldBeResizable()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);
        var inventory = player.Inventory.CreatePrimary(100);

        inventory.HasSpace(200).Should().BeFalse();
        inventory.Size = 500;
        inventory.HasSpace(200).Should().BeTrue();
    }

    [Fact]
    public async Task YouShouldBeAbleToCheckIfItemWillFitIntoInventory()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);
        var inventory = player.Inventory.CreatePrimary(100);

        inventory.Size = 100;
        inventory.HasSpaceForItem(itemsCollection, 1).Should().BeTrue();
        inventory.HasSpaceForItem(itemsCollection, 4).Should().BeTrue();
        inventory.HasSpaceForItem(itemsCollection, 5).Should().BeFalse();
        inventory.HasSpaceForItem(itemsCollection, 1, 100).Should().BeTrue();
        inventory.HasSpaceForItem(itemsCollection, 1, 101).Should().BeFalse();
    }

    private void HandleItemUsed(Inventory inventory, InventoryItem usedItem, ItemAction flags)
    {
        usedItem.SetMetadata("counter", usedItem.GetMetadata<int>("counter") - 1);
    }

    [Fact]
    public async Task YouShouldBeAbleToUseItem()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);
        var inventory = player.Inventory.CreatePrimary(100);

        inventory.ItemUsed += HandleItemUsed;
        inventory.Size = 100;
        var item = inventory.AddSingleItem(itemsCollection, 1, new ItemMetadata { ["counter"] = 10 });

        inventory.TryUseItem(item, ItemAction.Use).Should().BeTrue();
        item.GetMetadata("counter").Should().Be(9);
    }

    [Fact]
    public async Task YouCanNotUseItemIfItDoesNotSupportIt()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);
        var inventory = player.Inventory.CreatePrimary(100);

        inventory.Size = 100;
        var item = inventory.AddSingleItem(itemsCollection, 1);
        inventory.TryUseItem(item, ItemAction.Close).Should().BeFalse();
    }

    [Fact]
    public async Task RemoveItemShouldWork()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);
        var inventory = player.Inventory.CreatePrimary(100);

        var item = inventory.AddSingleItem(itemsCollection, 1);
        inventory.RemoveItem(item);
        inventory.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveItemRemoveMultipleStacks()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);
        var inventory = player.Inventory.CreatePrimary(100);

        inventory.AddItem(itemsCollection, 1, 20);
        inventory.RemoveItem(1, 20);
        inventory.Number.Should().Be(0);
        inventory.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveItemByIdShouldWork()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);
        var inventory = player.Inventory.CreatePrimary(100);

        inventory.AddSingleItem(itemsCollection, 1);
        inventory.RemoveItem(1);
        inventory.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveItemByIdShouldNotRemoveAnyStackIfThereIsNotEnough()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);
        var inventory = player.Inventory.CreatePrimary(100);

        inventory.AddItem(itemsCollection, 1, 20);
        inventory.RemoveItem(30);
        inventory.Number.Should().Be(20);
    }

    [Fact]
    public async Task RemoveItemShouldRemoveSingleItemWork()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);
        var inventory = player.Inventory.CreatePrimary(100);

        inventory.Size = 100;
        inventory.AddItem(itemsCollection, 1, 4);
        inventory.RemoveItem(1);

        var items = inventory.Items;
        items.Should().HaveCount(1);
        items[0].Number.Should().Be(3);
    }

    [Fact]
    public async Task RemoveItemStackShouldRemoveEntireStack()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);
        var inventory = player.Inventory.CreatePrimary(100);

        inventory.Size = 100;
        inventory.AddItem(itemsCollection, 1, 4);
        inventory.RemoveItemStack(1);

        inventory.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task AddItemShouldThrowAppropriateException()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);
        var inventory = player.Inventory.CreatePrimary(100);

        inventory.Size = 5;
        var act = () => inventory.AddItem(itemsCollection, 1, 4);

        act.Should().NotThrow();
        act.Should().Throw<InventoryNotEnoughSpaceException>().Where(x => x.InventorySize == 5 && x.RequiredSpace == 8);
    }

    [Fact]
    public async Task Clear()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);
        var inventory = player.Inventory.CreatePrimary(100);

        #region Arrange
        inventory.AddItem(itemsCollection, 1, 2);
        inventory.AddItem(itemsCollection, 2, 3);
        inventory.AddItem(itemsCollection, 3, 4);
        #endregion

        #region Act
        inventory.Clear();
        #endregion

        #region Assert
        inventory.Number.Should().Be(0);
        inventory.Items.Should().BeEmpty();
        #endregion
    }

    [InlineData(1, true, 9, 1)]
    [InlineData(10, true, 0, 10)]
    [InlineData(11, false, 10, 0)]
    [Theory]
    public async Task TransferItemTest(uint numberOfItemsToTransfer, bool success, int expectedNumberOfItemsInSourceInventory, int expectedNumberOfItemsInDestinationInventory)
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);
        var inventory = player.Inventory.CreatePrimary(100);

        #region Arrange
        inventory.Clear();
        var destinationInventory = new Inventory(player, 10);
        var metaData = new ItemMetadata
        {
            ["foo"] = 1
        };
        inventory.AddItem(itemsCollection, 1, 10, metaData);
        #endregion

        #region Act
        var isSuccess = inventory.TransferItem(destinationInventory, itemsCollection, 1, numberOfItemsToTransfer, false);
        #endregion

        #region Assert
        isSuccess.Should().Be(success);
        inventory.Number.Should().Be(expectedNumberOfItemsInSourceInventory);
        destinationInventory.Number.Should().Be(expectedNumberOfItemsInDestinationInventory);
        destinationInventory.Items.Select(x => x.MetaData).Should().AllBeEquivalentTo(metaData);
        #endregion
    }

    [Fact]
    public async Task TryGetMetaDataShouldWork()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);
        var inventory = player.Inventory.CreatePrimary(100);

        var item = inventory.AddSingleItem(itemsCollection, 1, new ItemMetadata
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
    public async Task TestHasItemWithCallback(uint itemId, bool has)
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);
        var inventory = player.Inventory.CreatePrimary(100);

        inventory.AddSingleItem(itemsCollection, 1);
        inventory.HasItem(x => x.ItemId == itemId).Should().Be(has);
    }

    [Fact]
    public async Task ItShouldBePossibleToCreateNewInventoryWithItems()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);

        var inventory = new Inventory(player, 10, 0, [
            new(itemsCollection, 1, 1)
        ]);
        inventory.Number.Should().Be(1);
    }

    [Fact]
    public async Task ItemMetaDataPropertyShouldReturnCopyOfMetaData()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);
        var inventory = player.Inventory.CreatePrimary(100);

        var metaData = new ItemMetadata
        {
            ["foo"] = 1
        };

        var item = inventory.AddSingleItem(itemsCollection, 1, metaData);

        item.MetaData.Should().BeEquivalentTo(metaData);
    }

    [Fact]
    public async Task ItemMetaDataKeysPropertyShouldReturnCopyOfMetaDataKeys()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);
        var inventory = player.Inventory.CreatePrimary(100);

        var metaData = new ItemMetadata
        {
            ["foo"] = 1
        };

        var item = inventory.AddSingleItem(itemsCollection, 1, metaData);

        item.MetadataKeys.Should().BeEquivalentTo(new List<string> { "foo" });
    }

    [Fact]
    public async Task YouShouldBeAbleToRemoveMetaData()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);
        var inventory = player.Inventory.CreatePrimary(100);

        var metaData = new ItemMetadata
        {
            ["foo"] = 1
        };

        var item = inventory.AddSingleItem(itemsCollection, 1, metaData);
        using var monitoredItem = item.Monitor();

        item.RemoveMetadata("foo");

        monitoredItem.Should().Raise(nameof(InventoryItem.MetadataRemoved));
        item.MetaData.Should().BeEmpty();
    }

    [Fact]
    public async Task YouShouldBeAbleToChangeMetaData()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);
        var inventory = player.Inventory.CreatePrimary(100);

        var metaData = new ItemMetadata
        {
            ["foo"] = 1
        };

        var item = inventory.AddSingleItem(itemsCollection, 1, metaData);
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
    public async Task HasMetadataShouldWork()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);
        var inventory = player.Inventory.CreatePrimary(100);

        var metaData = new ItemMetadata
        {
            ["foo"] = 1
        };

        var item = inventory.AddSingleItem(itemsCollection, 1, metaData);

        item.HasMetadata("foo").Should().BeTrue();
    }

    [Fact]
    public async Task TransferItemShouldBeThreadSafe()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);
        var inventory = player.Inventory.CreatePrimary(100);

        #region Arrange
        inventory.Clear();
        var destinationInventory = new Inventory(player, 800);
        inventory.AddItem(itemsCollection, 1, 800, null, true, true);
        #endregion

        #region Act
        await ParallelHelpers.Run(() =>
        {
            var isSuccess = inventory.TransferItem(destinationInventory, itemsCollection, 1, 1, false);
        });
        #endregion

        #region Assert
        inventory.Number.Should().Be(0);
        destinationInventory.IsFull.Should().BeTrue();
        #endregion
    }

    [Fact]
    public async Task RemoveAndGetItemByIdShouldWork()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);
        var inventory = player.Inventory.CreatePrimary(100);

        var metaData = new ItemMetadata
        {
            ["foo"] = 1
        };

        var addedItem = inventory.AddSingleItem(itemsCollection, 1, metaData);
        var removedItem = inventory.RemoveAndGetItemById(1).First();

        addedItem.Should().BeEquivalentTo(removedItem);
    }

    [Fact]
    public async Task RemoveAndGetItemByIdShouldWorkForManyItems()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);
        var inventory = player.Inventory.CreatePrimary(100);

        var metaData = new ItemMetadata
        {
            ["foo"] = 1
        };

        var addedItem = inventory.AddItem(itemsCollection, 1, 20, metaData);
        var removedItem = inventory.RemoveAndGetItemById(1, 20);

        addedItem.Should().BeEquivalentTo(removedItem);
    }

    [Fact]
    public async Task RemovingOneItemAndGetItemByIdShouldWorkForManyItems()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);
        var inventory = player.Inventory.CreatePrimary(100);

        var metaData = new ItemMetadata
        {
            ["foo"] = 1
        };

        inventory.AddItem(itemsCollection, 1, 20, metaData);
        var removedItem = inventory.RemoveAndGetItemById(1, 1);
        inventory.Number.Should().Be(19);
        removedItem.Should().HaveCount(1);
        removedItem.First().MetaData.Should().BeEquivalentTo(metaData);
    }

    [Fact]
    public async Task YouShouldBeAbleToModifyInventoryInInventoryChangedEventCallback()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);

        var inventory = player.Inventory.CreatePrimary(100);

        void handleItemAdded(Inventory that, InventoryItem addedItem)
        {
            if(that.Number < 5)
            {
                that.AddSingleItem(itemsCollection, 1);
            }
        }

        inventory.ItemChanged += handleItemAdded;

        inventory.AddSingleItem(itemsCollection, 1);
        inventory.Number.Should().Be(5);
    }

    [Fact]
    public async Task AddingItemObjectsToInventoryShouldWork()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);

        var item1 = new InventoryItem(itemsCollection, 1, 2);
        var item2 = new InventoryItem(itemsCollection, 1, 2);

        var inventory = player.Inventory.CreatePrimary(100);

        inventory.AddItem(itemsCollection, item1);
        inventory.AddItem(itemsCollection, item2);
        inventory.AddItem(itemsCollection, item1);
        inventory.AddItem(itemsCollection, item2);

        var items = inventory.GetItemsById(1);
        items.Should().HaveCount(1);

        items.First().Number.Should().Be(8);
    }

    [Fact]
    public async Task ItemsOfDifferentMetadataShouldNotStack()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var itemsCollection = hosting.GetRequiredService<ItemsCollection>();
        Seed(hosting);

        var item1 = new InventoryItem(itemsCollection, 1, 2, new(){
            ["a"] = 1
        });
        var item2 = new InventoryItem(itemsCollection, 1, 2);

        var inventory = player.Inventory.CreatePrimary(100);

        inventory.AddItem(itemsCollection, item1);
        inventory.AddItem(itemsCollection, item2);
        inventory.AddItem(itemsCollection, item1);
        inventory.AddItem(itemsCollection, item2);

        var items = inventory.GetItemsById(1);
        items.Should().HaveCount(2);

        items.First().Number.Should().Be(4);
        items.Last().Number.Should().Be(4);
    }
}
