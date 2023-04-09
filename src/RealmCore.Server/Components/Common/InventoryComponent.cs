using RealmCore.Server.Enums;

namespace RealmCore.Server.Components.Common;

[ComponentUsage(true)]
public class InventoryComponent : Component
{
    private readonly List<Item> _items = new();
    private readonly object _itemsLock = new();
    public event Action<InventoryComponent, Item>? ItemAdded;
    public event Action<InventoryComponent, Item>? ItemRemoved;
    public event Action<InventoryComponent, Item>? ItemChanged;
    public event Action<InventoryComponent, Item, ItemAction>? ItemUsed;

    private decimal _size;
    internal int Id { get; set; }
    public decimal Size
    {
        get
        {
            ThrowIfDisposed();
            return _size;
        }
        set
        {
            ThrowIfDisposed();
            _size = value;
        }
    }

    public decimal Number
    {
        get
        {
            ThrowIfDisposed();
            lock (_itemsLock)
                return (uint)_items.Sum(x => x.Size * x.Number);
        }
    }

    public IReadOnlyList<Item> Items
    {
        get
        {
            ThrowIfDisposed();
            lock (_itemsLock)
                return new List<Item>(_items);
        }
    }

    public InventoryComponent(decimal size)
    {
        _size = size;
    }

    internal InventoryComponent(decimal size, int id, IEnumerable<Item> items)
    {
        _size = size;
        Id = id;
        _items = items.ToList();
    }

    public bool HasItemById(uint itemId)
    {
        ThrowIfDisposed();
        lock (_itemsLock)
            return _items.Any(x => x.ItemId == itemId);
    }

    internal bool InternalHasItem(Item item) => _items.Contains(item);

    public bool HasItem(Item item)
    {
        ThrowIfDisposed();
        lock (_itemsLock)
            return InternalHasItem(item);
    }

    public int SumItemsById(uint itemId)
    {
        ThrowIfDisposed();
        lock (_itemsLock)
            return _items.Count(x => x.ItemId == itemId);
    }

    public IReadOnlyList<Item> GetItemsById(uint itemId)
    {
        ThrowIfDisposed();
        lock (_itemsLock)
            return new List<Item>(_items.Where(x => x.ItemId == itemId));
    }

    public IReadOnlyList<Item> GetItemsByIdWithMetadata(uint itemId, string key, object? metadata)
    {
        ThrowIfDisposed();
        lock (_itemsLock)
            return new List<Item>(_items.Where(x => x.ItemId == itemId && x.GetMetadata(key).Equals(metadata)));
    }

    public bool HasItemWithMetadata(uint itemId, string key, object? metadata)
    {
        ThrowIfDisposed();
        lock (_itemsLock)
            return _items.Any(x => x.ItemId == itemId && x.GetMetadata(key).Equals(metadata));
    }

    public bool TryGetByLocalId(string localId, out Item item)
    {
        ThrowIfDisposed();
        lock (_itemsLock)
            item = _items.FirstOrDefault(x => x.LocalId == localId)!;
        return item != null;
    }

    public bool TryGetByItemId(uint itemId, out Item item)
    {
        ThrowIfDisposed();
        lock (_itemsLock)
            item = _items.FirstOrDefault(x => x.ItemId == itemId)!;
        return item != null;
    }

    public bool TryGetByIdAndMetadata(uint itemId, Dictionary<string, object> metadata, out Item item)
    {
        ThrowIfDisposed();
        lock (_itemsLock)
            item = _items.FirstOrDefault(x => x.ItemId == itemId && x.Equals(metadata))!;
        return item != null;
    }

    public Item AddSingleItem(ItemsRegistry itemsRegistry, uint itemId, Dictionary<string, object>? metadata = null, bool tryStack = true, bool force = false)
    {
        ThrowIfDisposed();
        return AddItem(itemsRegistry, itemId, 1, metadata, tryStack, force).First();
    }

    public IEnumerable<Item> AddItem(ItemsRegistry itemsRegistry, uint itemId, uint number = 1, Dictionary<string, object>? metadata = null, bool tryStack = true, bool force = false)
    {
        ThrowIfDisposed();
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

    public bool RemoveItemStack(Item item) => RemoveItem(item, item.Number);
    public bool RemoveItemStack(uint id)
    {
        ThrowIfDisposed();
        if (TryGetByItemId(id, out var item))
            return RemoveItemStack(item);
        return false;
    }

    public bool RemoveItem(Item item, uint number = 1)
    {
        ThrowIfDisposed();
        if (number == 0 || !HasItem(item))
            return false;

        if (item.Number > number)
        {
            item.Number -= number;
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
        return true;
    }

    public bool RemoveItem(uint id, uint number = 1)
    {
        ThrowIfDisposed();
        if (TryGetByItemId(id, out var item))
            return RemoveItem(item, number);
        return false;
    }

    public bool TryUseItem(Item item, ItemAction flags)
    {
        ThrowIfDisposed();

        if (HasItem(item) && (item.AvailiableActions & flags) == flags)
        {
            ItemUsed?.Invoke(this, item, flags);
            return true;
        }

        return false;
    }

    public bool HasSpace(decimal space)
    {
        return Number + space <= Size;
    }
    
    public bool HasSpaceForItem(uint itemId, ItemsRegistry itemsRegistry)
    {
        return Number + itemsRegistry.Get(itemId).Size <= Size;
    }
    
    public bool HasSpaceForItem(uint itemId, uint number, ItemsRegistry itemsRegistry)
    {
        return Number + itemsRegistry.Get(itemId).Size * number <= Size;
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}
