using RealmCore.Server.Inventory;

namespace RealmCore.Tests.Tests.Components;

public class InventoryComponentTests
{
    private readonly ItemsRegistry _itemsRegistry;
    private readonly Entity _entity;
    private readonly InventoryComponent _inventoryComponent;

    public InventoryComponentTests()
    {
        _itemsRegistry = new();
        _itemsRegistry.AddItem(1, new ItemRegistryEntry
        {
            Name = "test item id 1",
            Size = 1,
            StackSize = 1,
            AvailiableActions = ItemAction.Use,
        });
        _itemsRegistry.AddItem(2, new ItemRegistryEntry
        {
            Name = "test item id 2",
            Size = 2,
            StackSize = 1,
            AvailiableActions = ItemAction.Use,
        });
        _itemsRegistry.AddItem(3, new ItemRegistryEntry
        {
            Name = "test item id 3",
            Size = 1,
            StackSize = 8,
            AvailiableActions = ItemAction.Use,
        });
        _itemsRegistry.AddItem(4, new ItemRegistryEntry
        {
            Name = "test item id 4",
            Size = 2,
            StackSize = 8,
            AvailiableActions = ItemAction.Use,
        });

        var services = new ServiceCollection();
        services.AddSingleton<IRealmConfigurationProvider>(new TestConfigurationProvider());
        _entity = new(services.BuildServiceProvider(), "test", EntityTag.Unknown);
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
        _inventoryComponent.AddItem(_itemsRegistry, 1, 1);
        _inventoryComponent.AddItem(_itemsRegistry, 1, 1);
        _inventoryComponent.AddItem(_itemsRegistry, 2, 1);

        _inventoryComponent.SumItemsById(1).Should().Be(2);
    }
    
    [Fact]
    public void GetItemsByIdShouldWork()
    {
        var item1 = _inventoryComponent.AddItem(_itemsRegistry, 1, 1).First();
        var item2 = _inventoryComponent.AddItem(_itemsRegistry, 1, 1).First();
        _inventoryComponent.AddItem(_itemsRegistry, 2, 1);

        _inventoryComponent.GetItemsById(1).Should().BeEquivalentTo(new List<Item>
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
    public void HasItemWithMetadataShouldWork()
    {
        var item = _inventoryComponent.AddSingleItem(_itemsRegistry, 1, new Dictionary<string, object> { ["foo"] = 1 });
        _inventoryComponent.AddItem(_itemsRegistry, 1, 1, new Dictionary<string, object> { ["foo"] = 2 });
        _inventoryComponent.AddItem(_itemsRegistry, 1, 1, new Dictionary<string, object> { ["foo"] = 3 });

        _inventoryComponent.HasItemWithMetadata(1, "foo", 1).Should().BeTrue();
        _inventoryComponent.HasItemWithMetadata(1, "foo", 4).Should().BeFalse();
    }
    
    [Fact]
    public void asd()
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
}
