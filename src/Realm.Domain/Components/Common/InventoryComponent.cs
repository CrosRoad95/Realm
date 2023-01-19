namespace Realm.Domain.Components.Common;

public class InventoryComponent : Component
{
    [Inject]
    private ItemsRegistry ItemsRegistry { get; set; } = default!;
    [Inject]
    private ILogger Logger { get; set; } = default!;

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

    internal InventoryComponent(InventoryData inventory, ItemsRegistry itemsRegistry, ILogger logger)
    {
        if (inventory == null)
            throw new ArgumentNullException(nameof(inventory));

        Size = inventory.Size;
        Id = inventory.Id;

        foreach (var item in inventory.InventoryItems)
        {
            try
            {
                _items.Add(new Item(item, itemsRegistry.Get(item.ItemId)));
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Failed to load item {item}", item.Id);
            }
        }
    }

    public ItemRegistryEntry GetItemRegistryEntry(uint id) => ItemsRegistry!.Get(id);

    public bool HasItem(uint itemId)
    {
        return _items.Any(x => x.ItemId == itemId);
    }

    public bool TryGetByLocalId(string localId, out Item item)
    {
        item = _items.FirstOrDefault(x => x.LocalId == localId)!;
        return item != null;
    }
    
    public bool TryGetByItemId(uint itemId, out Item item)
    {
        item = _items.FirstOrDefault(x => x.ItemId == itemId)!;
        return item != null;
    }

    public bool TryGetByIdAndMetadata(uint itemId, Dictionary<string, object> metadata, out Item item)
    {
        item = _items.FirstOrDefault(x => x.ItemId == itemId && x.Equals(metadata))!;
        return item != null;
    }

    public void AddItem(uint itemId, uint number = 1, Dictionary<string, object>? metadata = null, bool tryStack = true, bool force = false)
    {
        if (number == 0)
            return;

        var itemRegistryEntry = ItemsRegistry.Get(itemId);
        if (Number + itemRegistryEntry.Size * number > Size && !force)
            throw new InventoryDoNotEnoughSpaceException();

        List<Item> newItems = new();
        if (tryStack)
        {
            foreach (var item in _items.Where(x => x.ItemId == itemId))
            {
                if(metadata != null)
                {
                    if (!item.Equals(metadata))
                        break;
                }

                var added = Math.Min(number, itemRegistryEntry.StackSize - item.Number);
                item.Number += added;
                number -= added;
            }
        }
        else
        {
            var item = new Item(itemRegistryEntry);
            newItems.Add(item);
        }

        while (number > 0)
        {
            var thisItemNumber = Math.Min(number, itemRegistryEntry.StackSize);
            number -= thisItemNumber;
            var item = new Item(itemRegistryEntry);
            item.Number = thisItemNumber;
            newItems.Add(item);
            ;
        }

        foreach (var newItem in newItems)
        {
            newItem.Changed += HandleItemChanged;
            _items.Add(newItem);
            ItemAdded?.Invoke(this, newItem);
        }
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

    public async Task<bool> TryUseItem(Item item, ItemRegistryEntry.ItemAction action)
    {
        if (item.AvailiableActions.HasFlag(action))
        {
            await ItemsRegistry.UseCallback(this, item, action);
            return true;
        }

        return false;
    }
}
