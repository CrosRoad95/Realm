namespace Realm.Domain.Components.Common;

public class InventoryComponent : Component
{

    [Inject]
    private ItemsRegistry ItemsRegistry { get; set; } = default!;

    private readonly List<Item> _items = new();
    public event Action<InventoryComponent, Item>? ItemAdded;
    public Guid Id { get; private set; } = Guid.NewGuid();
    public uint Size { get; set; }
    public uint Number => (uint)_items.Sum(x => ItemsRegistry!.Get(x.ItemId).Size * x.Number);

    public IEnumerable<Item> Items => _items;

    public InventoryComponent(uint size)
    {
        Size = size;
    }

    public InventoryComponent(InventoryData inventory)
    {
        if (inventory == null)
            throw new ArgumentNullException(nameof(inventory));

        Size = inventory.Size;
        Id = inventory.Id;
        _items = inventory.InventoryItems.Select(x => new Item(x)).ToList();
        foreach (var item in _items)
            item.InventoryComponent = this;
    }
    public ItemRegistryEntry GetItemRegistryEntry(uint id) => ItemsRegistry!.Get(id);

    public bool HasItem(uint itemId)
    {
        return _items.Any(x => x.ItemId == itemId);
    }

    public Item? AddItem(uint itemId)
    {
        var itemRegistryEntry = ItemsRegistry.Get(itemId);
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
