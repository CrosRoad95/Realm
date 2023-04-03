using Realm.Domain.Inventory;

namespace Realm.Domain.Components.Common;

[ComponentUsage(true)]
public class InventoryComponent : Component
{
    private readonly List<Item> _items = new();
    private readonly object _itemsLock = new();
    public event Action<InventoryComponent, Item>? ItemAdded;
    public event Action<InventoryComponent, Item>? ItemRemoved;
    public event Action<InventoryComponent, Item>? ItemChanged;

    internal int Id { get; set; }
    public decimal Size { get; set; }
    public decimal Number
    {
        get
        {
            lock (_itemsLock)
                return (uint)_items.Sum(x => x.Size * x.Number);
        }
    }

    public IReadOnlyList<Item> Items {
        get {
            lock(_itemsLock)
                return new List<Item>(_items);
        }
    }

    public Func<InventoryComponent, Item, ItemAction, Task> UseCallback { get; set; } = default!;

    public InventoryComponent(decimal size)
    {
        Size = size;
    }

    internal InventoryComponent(decimal size, int id, IEnumerable<Item> items)
    {
        Size = size;
        Id = id;
        _items = items.ToList();
    }

    public bool HasItemById(uint itemId)
    {
        lock(_itemsLock)
            return _items.Any(x => x.ItemId == itemId);
    }
    
    public int SumItemsById(uint itemId)
    {
        lock (_itemsLock)
            return _items.Count(x => x.ItemId == itemId);
    }
    
    public IReadOnlyList<Item> GetItemsById(uint itemId)
    {
        lock (_itemsLock)
            return new List<Item>(_items.Where(x => x.ItemId == itemId));
    }
    
    public IReadOnlyList<Item> GetItemsByIdWithMetadata(uint itemId, string key, object? metadata)
    {
        lock (_itemsLock)
            return new List<Item>(_items.Where(x => x.ItemId == itemId && x.GetMetadata(key).Equals(metadata)));
    }

    public bool HasItemWithMetadata(uint itemId, string key, object? metadata)
    {
        lock (_itemsLock)
            return _items.Any(x => x.ItemId == itemId && x.GetMetadata(key).Equals(metadata));
    }

    public bool TryGetByLocalId(string localId, out Item item)
    {
        lock (_itemsLock)
            item = _items.FirstOrDefault(x => x.LocalId == localId)!;
        return item != null;
    }
    
    public bool TryGetByItemId(uint itemId, out Item item)
    {
        lock (_itemsLock)
            item = _items.FirstOrDefault(x => x.ItemId == itemId)!;
        return item != null;
    }

    public bool TryGetByIdAndMetadata(uint itemId, Dictionary<string, object> metadata, out Item item)
    {
        lock (_itemsLock)
            item = _items.FirstOrDefault(x => x.ItemId == itemId && x.Equals(metadata))!;
        return item != null;
    }

    public Item AddSingleItem(ItemsRegistry itemsRegistry, uint itemId, Dictionary<string, object>? metadata = null, bool tryStack = true, bool force = false)
    {
        return AddItem(itemsRegistry, itemId, 1, metadata, tryStack, force).First();
    }

    public IEnumerable<Item> AddItem(ItemsRegistry itemsRegistry, uint itemId, uint number = 1, Dictionary<string, object>? metadata = null, bool tryStack = true, bool force = false)
    {
        if (number == 0)
            throw new ArgumentOutOfRangeException(nameof(number));

        if (metadata == null)
            metadata = new();

        var itemRegistryEntry = itemsRegistry.Get(itemId);
        if (Number + itemRegistryEntry.Size * number > Size && !force)
            throw new InventoryDoNotEnoughSpaceException();

        List<Item> newItems = new();    
        if (tryStack)
        {
            lock (_itemsLock)
            {
                foreach (var item in _items.Where(x => x.ItemId == itemId))
                {
                    if (!item.Equals(metadata))
                        continue;

                    var added = Math.Min(number, itemRegistryEntry.StackSize - item.Number);
                    item.Number += added;
                    number -= added;
                }
            }
        }

        while (number > 0)
        {
            var thisItemNumber = Math.Min(number, itemRegistryEntry.StackSize);
            number -= thisItemNumber;
            var item = new Item(itemsRegistry, itemId, number, metadata);
            item.Number = thisItemNumber;
            newItems.Add(item);
        }

        foreach (var newItem in newItems)
        {
            newItem.Changed += HandleItemChanged;
            lock (_itemsLock)
                _items.Add(newItem);
            ItemAdded?.Invoke(this, newItem);
        }

        return newItems.AsReadOnly();
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
            lock (_itemsLock)
                if (!_items.Remove(item))
                    throw new InvalidOperationException();

            item.Changed -= HandleItemChanged;
            ItemRemoved?.Invoke(this, item);
        }
    }
    
    public void RemoveItem(uint id, bool removeEntireStack = false)
    {
        if (TryGetByItemId(id, out var item))
            RemoveItem(item, removeEntireStack);
    }

    public async Task<bool> TryUseItem(Item item, ItemAction action)
    {
        if (item.AvailiableActions.HasFlag(action))
        {
            await UseCallback(this, item, action);
            return true;
        }

        return false;
    }
}
