namespace Realm.Domain.Components.Common;

public class InventoryComponent : Component
{
    [Inject]
    private ItemsRegistry ItemsRegistry { get; set; } = default!;

    private readonly List<Item> _items = new();
    public event Action<InventoryComponent, Item>? ItemAdded;
    public event Action<InventoryComponent, Item>? ItemRemoved;
    public event Action<InventoryComponent, Item>? ItemChanged;

    public Guid? Id { get; private set; } = null;
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

    public bool TryGetById(uint itemId, out Item item)
    {
        item = _items.FirstOrDefault(x => x.ItemId == itemId)!;
        return item != null;
    }

    public Item? AddItem(uint itemId, uint number = 1)
    {
        var itemRegistryEntry = ItemsRegistry.Get(itemId);
        var item = new Item(itemRegistryEntry);
        item.Number = Math.Min(itemRegistryEntry.StackSize, number);
        AddItem(item);
        item.Changed += HandleItemChanged;
        ItemAdded?.Invoke(this, item);
        return item;
    }

    private void HandleItemChanged(Item item)
    {
        ItemChanged?.Invoke(this, item);
    }

    public void RemoveItem(Item item, bool removeEntireStack = false)
    {
        if(item.Number > 1 && !removeEntireStack)
        {
            item.Number--;
            ItemChanged?.Invoke(this, item);
        }
        else
        {
            if(!_items.Remove(item))
                throw new InvalidOperationException();

            ItemRemoved?.Invoke(this, item);
        }
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

    public async Task<bool> TryUseItem(uint itemId, ItemRegistryEntry.ItemAction action)
    {
        if(TryGetById(itemId, out var item)){
            var itemEntry = ItemsRegistry.Get(itemId);
            if(itemEntry.AvailiableActions.HasFlag(action))
            {
                await ItemsRegistry.UseCallback(this, item, action);
                return true;
            }
        }

        return false;
    }
}
