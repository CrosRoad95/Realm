using Realm.Domain.Inventory;
using Realm.Domain.Registries;
using InventoryData = Realm.Persistance.Data.Inventory;

namespace Realm.Domain.Components.Common;

public class InventoryComponent : Component
{
    private readonly List<Item> _items = new();
    public event Action<InventoryComponent, Item>? ItemAdded;
    private ItemsRegistry? _itemsRegistry;
    public string Id { get; private set; } = Guid.NewGuid().ToString();
    public uint Size { get; set; }
    public uint Number => (uint)_items.Sum(x => _itemsRegistry!.Get(x.ItemId).Size * x.Number);

    public IEnumerable<Item> Items => _items;

    public InventoryComponent(uint size)
    {
        Size = size;
    }

    public InventoryComponent(InventoryData? inventory)
    {
        if (inventory == null)
            throw new ArgumentNullException();

        Size = inventory.Size;
        Id = inventory.Id;
        _items = inventory.InventoryItems.Select(x => new Item(x)).ToList();
        foreach (var item in _items)
            item.InventoryComponent = this;
    }

    public ItemRegistryEntry GetItemRegistryEntry(uint id) => _itemsRegistry!.Get(id);

    public override Task Load()
    {
        _itemsRegistry = Entity.GetRequiredService<ItemsRegistry>();
        return Task.CompletedTask;
    }

    public bool HasItem(uint itemId)
    {
        return _items.Any(x => x.ItemId == itemId);
    }

    public Item? AddItem(uint itemId)
    {
        var itemRegistryEntry = _itemsRegistry.Get(itemId);
        var item = new Item(itemRegistryEntry);
        AddItem(item);
        return item;
    }

    public Item? AddItem(Item item)
    {
        if (item.InventoryComponent != null)
            throw new Exception("Item already used in other inventory component");

        if (item.Size + Number > Size)
            return null;
        _items.Add(item);
        item.InventoryComponent = this;
        ItemAdded?.Invoke(this, item);
        return item;
    }
}
