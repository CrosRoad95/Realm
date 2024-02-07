namespace RealmCore.Server.Concepts;

public class Inventory
{
    private readonly List<Item> _items = [];
    private readonly ReaderWriterLockSlim _semaphore = new(LockRecursionPolicy.SupportsRecursion);
    public event Action<Inventory, Item>? ItemAdded;
    public event Action<Inventory, Item>? ItemRemoved;
    public event Action<Inventory, Item>? ItemChanged;
    public event Action<Inventory, Item, ItemAction>? ItemUsed;
    public event Action<Inventory, decimal>? SizeChanged;

    private decimal _size;

    public Element Owner { get; }
    internal int Id { get; set; }

    public decimal Size
    {
        get
        {
            return _size;
        }
        set
        {
            SizeChanged?.Invoke(this, value);
            _size = value;
        }
    }

    public bool IsFull => Number >= Size;

    public decimal Number
    {
        get
        {
            _semaphore.EnterReadLock();
            try
            {
                return _items.Sum(x => x.Size * x.Number);
            }
            finally
            {
                _semaphore.ExitReadLock();
            }
        }
    }

    public IReadOnlyList<Item> Items
    {
        get
        {
            _semaphore.EnterReadLock();
            try
            {
                return new List<Item>(_items);
            }
            finally
            {
                _semaphore.ExitReadLock();
            }
        }
    }

    public Inventory(Element owner, decimal size)
    {
        Owner = owner;
        _size = size;
    }

    internal Inventory(Element owner, decimal size, int id, IEnumerable<Item> items)
    {
        Owner = owner;
        _size = size;
        Id = id;
        _items = items.ToList();
    }

    public bool HasItemById(uint itemId)
    {
        _semaphore.EnterReadLock();
        try
        {
            return _items.Any(x => x.ItemId == itemId);
        }
        finally
        {
            _semaphore.ExitReadLock();
        }
    }

    public bool HasItem(Func<Item, bool> callback)
    {
        _semaphore.EnterReadLock();
        try
        {
            return _items.Any(callback);
        }
        finally
        {
            _semaphore.ExitReadLock();
        }
    }

    public bool HasItem(Item item)
    {
        _semaphore.EnterReadLock();
        try
        {
            return _items.Contains(item);
        }
        finally
        {
            _semaphore.ExitReadLock();
        }
    }

    public int SumItemsById(uint itemId)
    {
        _semaphore.EnterReadLock();
        try
        {
            return _items.Count(x => x.ItemId == itemId);
        }
        finally
        {
            _semaphore.ExitReadLock();
        }
    }

    public int SumItemsNumberById(uint itemId)
    {
        _semaphore.EnterReadLock();
        try
        {
            return (int)_items.Where(x => x.ItemId == itemId).Sum(x => x.Number);
        }
        finally
        {
            _semaphore.ExitReadLock();
        }
    }

    public IReadOnlyList<Item> GetItemsById(uint itemId)
    {
        _semaphore.EnterReadLock();
        try
        {
            return new List<Item>(_items.Where(x => x.ItemId == itemId));
        }
        finally
        {
            _semaphore.ExitReadLock();
        }
    }

    public IReadOnlyList<Item> GetItemsByIdWithMetadata(uint itemId, string key, object? metadata)
    {
        _semaphore.EnterReadLock();
        try
        {
            return new List<Item>(_items.Where(x => x.ItemId == itemId && (x.GetMetadata(key)?.Equals(metadata) ?? false)));
        }
        finally
        {
            _semaphore.ExitReadLock();
        }
    }

    public Item? GetSingleItemByIdWithMetadata(uint itemId, Metadata metadata)
    {
        _semaphore.EnterReadLock();
        try
        {
            return _items.Where(x => x.ItemId == itemId && x.Equals(metadata)).FirstOrDefault();
        }
        finally
        {
            _semaphore.ExitReadLock();
        }
    }

    public bool HasItemWithMetadata(uint itemId, string key, object? metadata)
    {
        _semaphore.EnterReadLock();
        try
        {
            return _items.Any(x => x.ItemId == itemId && (x.GetMetadata(key)?.Equals(metadata) ?? false));
        }
        finally
        {
            _semaphore.ExitReadLock();
        }
    }

    public bool TryGetByLocalId(string localId, out Item item)
    {
        _semaphore.EnterReadLock();
        try
        {
            item = _items.FirstOrDefault(x => x.LocalId == localId)!;
            return item != null;
        }
        finally
        {
            _semaphore.ExitReadLock();
        }
    }

    public bool TryGetByItemId(uint itemId, out Item item)
    {
        _semaphore.EnterReadLock();
        try
        {
            item = _items.FirstOrDefault(x => x.ItemId == itemId)!;
            return item != null;
        }
        finally
        {
            _semaphore.ExitReadLock();
        }
    }

    public bool TryGetByIdAndMetadata(uint itemId, Metadata metadata, out Item item)
    {
        _semaphore.EnterReadLock();
        try
        {
            item = _items.FirstOrDefault(x => x.ItemId == itemId && x.Equals(metadata))!;
            return item != null;
        }
        finally
        {
            _semaphore.ExitReadLock();
        }
    }

    public Item AddSingleItem(ItemsRegistry itemsRegistry, uint itemId, Metadata? metadata = null, bool tryStack = true, bool force = false)
    {
        _semaphore.EnterWriteLock();
        try
        {
            var item = AddItem(itemsRegistry, itemId, 1, metadata, tryStack, force);
            if (item.Any())
                return item.First();
            return GetSingleItemByIdWithMetadata(itemId, metadata ?? []) ?? throw new InvalidOperationException();
        }
        finally
        {
            _semaphore.ExitWriteLock();
        }
    }

    public IEnumerable<Item> AddItem(ItemsRegistry itemsRegistry, uint itemId, uint number = 1, Metadata? metadata = null, bool tryStack = true, bool force = false)
    {
        if (number <= 0)
            throw new ArgumentOutOfRangeException(nameof(number));

        metadata ??= [];

        var itemRegistryEntry = itemsRegistry.Get(itemId);
        var requiredSpace = Number + itemRegistryEntry.Size * number;
        if (requiredSpace > Size && !force)
            throw new InventoryNotEnoughSpaceException(Size, requiredSpace);

        _semaphore.EnterWriteLock();
        try
        {
            List<Item> newItems = [];
            if (tryStack)
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

            while (number > 0)
            {
                var thisItemNumber = Math.Min(number, itemRegistryEntry.StackSize);
                number -= thisItemNumber;
                var item = new Item(itemsRegistry, itemId, number, metadata)
                {
                    Number = thisItemNumber
                };
                newItems.Add(item);
            }

            foreach (var newItem in newItems)
            {
                newItem.MetadataChanged += HandleItemChanged;
                _items.Add(newItem);
                ItemAdded?.Invoke(this, newItem);
            }

            return newItems.AsReadOnly();
        }
        finally
        {
            _semaphore.ExitWriteLock();
        }
    }

    public void AddItem(ItemsRegistry itemsRegistry, Item newItem, bool force = false)
    {
        if (!force)
        {
            var itemRegistryEntry = itemsRegistry.Get(newItem.ItemId);
            var requiredSpace = Number + itemRegistryEntry.Size * newItem.Number;
            if (requiredSpace > Size)
                throw new InventoryNotEnoughSpaceException(Size, requiredSpace);
        }

        _semaphore.EnterWriteLock();
        try
        {
            newItem.MetadataChanged += HandleItemChanged;
            _items.Add(newItem);
            ItemAdded?.Invoke(this, newItem);
        }
        finally
        {
            _semaphore.ExitWriteLock();
        }
    }

    public void AddItems(ItemsRegistry itemsRegistry, IEnumerable<Item> newItems, bool force = false)
    {
        _semaphore.EnterWriteLock();

        if (!force)
        {
            decimal requiredSpace = 0;
            foreach (var newItem in newItems)
            {
                var itemRegistryEntry = itemsRegistry.Get(newItem.ItemId);
                requiredSpace += Number + itemRegistryEntry.Size * newItem.Number;
            }

            if (requiredSpace > Size)
                throw new InventoryNotEnoughSpaceException(Size, requiredSpace);
        }

        try
        {
            foreach (var newItem in newItems)
            {
                newItem.MetadataChanged += HandleItemChanged;
                _items.Add(newItem);
                ItemAdded?.Invoke(this, newItem);
            }
        }
        finally
        {
            _semaphore.ExitWriteLock();
        }
    }

    private void HandleItemChanged(Item item, string key)
    {
        ItemChanged?.Invoke(this, item);
    }

    public bool RemoveItemStack(Item item) => RemoveItem(item, item.Number);
    public bool RemoveItemStack(uint id)
    {
        _semaphore.EnterWriteLock();
        try
        {
            if (TryGetByItemId(id, out var item))
                return RemoveItemStack(item);
            return false;
        }
        finally
        {
            _semaphore.ExitWriteLock();
        }
    }

    public bool RemoveItem(Item item, uint number = 1)
    {
        _semaphore.EnterWriteLock();
        try
        {
            return RemoveItem(item, out var _, out var _, number);
        }
        finally
        {
            _semaphore.ExitWriteLock();
        }
    }

    public bool RemoveItem(Item item, out uint removedNumber, out bool stackRemoved, uint number = 1)
    {
        if (number <= 0)
            throw new ArgumentOutOfRangeException(nameof(number));

        _semaphore.EnterWriteLock();
        try
        {
            if (!HasItem(item))
            {
                removedNumber = 0;
                stackRemoved = false;
                return false;
            }

            if (item.Number > number)
            {
                item.Number -= number;
                removedNumber = number;
                stackRemoved = false;
                ItemChanged?.Invoke(this, item);
            }
            else
            {
                if (!_items.Remove(item))
                    throw new InvalidOperationException();

                item.MetadataChanged -= HandleItemChanged;
                removedNumber = item.Number;
                stackRemoved = true;
                ItemRemoved?.Invoke(this, item);
            }
            return true;
        }
        finally
        {
            _semaphore.ExitWriteLock();
        }
    }

    public bool RemoveItem(uint id, uint number = 1)
    {
        _semaphore.EnterWriteLock();
        try
        {
            if (SumItemsNumberById(id) >= number && TryGetByItemId(id, out var item))
            {
                var success = RemoveItem(item, out var removedNumber, out var _, number);
                var left = number - removedNumber;
                if (left > 0)
                    success = RemoveItem(id, left);
                return success;
            }
            return false;
        }
        finally
        {
            _semaphore.ExitWriteLock();
        }
    }

    public List<Item> RemoveAndGetItemById(uint id, uint number = 1)
    {
        return RemoveAndGetItemByIdInternal(id, number).ToList();
    }

    private IEnumerable<Item> RemoveAndGetItemByIdInternal(uint id, uint number = 1)
    {
        _semaphore.EnterWriteLock();
        try
        {
            if (SumItemsNumberById(id) >= number && TryGetByItemId(id, out var item))
            {
                var success = RemoveItem(item, out var removedNumber, out bool stackRemoved, number);
                if (item.Number > 0 && !stackRemoved)
                {
                    item = new Item(item)
                    {
                        Number = removedNumber
                    };
                }
                Debug.Assert(success == true);
                var left = number - removedNumber;
                if (left > 0)
                {
                    foreach (var removedItem in RemoveAndGetItemByIdInternal(id, left))
                    {
                        yield return removedItem;
                    }
                }
                yield return item;
            }
        }
        finally
        {
            _semaphore.ExitWriteLock();
        }
    }

    public bool TryUseItem(Item item, ItemAction flags)
    {
        _semaphore.EnterReadLock();
        try
        {
            if (HasItem(item) && (item.AvailableActions & flags) == flags)
            {
                ItemUsed?.Invoke(this, item, flags);
                return true;
            }

            return false;
        }
        finally
        {
            _semaphore.ExitReadLock();
        }
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

    public bool TransferItem(Inventory destination, ItemsRegistry itemsRegistry, uint itemId, uint number, bool force)
    {
        if (SumItemsNumberById(itemId) >= number || force)
        {
            var removedItems = RemoveAndGetItemById(itemId, number);
            try
            {
                destination.AddItems(itemsRegistry, removedItems, force: force);
                return true;
            }
            catch (InventoryNotEnoughSpaceException)
            {
                AddItems(itemsRegistry, removedItems);
                return false;
            }
        }
        return false;
    }

    public void Clear()
    {
        var items = Items.ToList();
        foreach (var item in items)
            RemoveItem(item, item.Number);
    }

    private static readonly JsonSerializerSettings _jsonSerializerSettings = new()
    {
        Converters = new List<JsonConverter> { DoubleConverter.Instance }
    };

    public static Inventory CreateFromData(Element element, InventoryData inventory, ItemsRegistry itemsRegistry)
    {
        var items = inventory.InventoryItems
            .Select(x =>
                new Item(itemsRegistry, x.ItemId, x.Number, JsonConvert.DeserializeObject<Metadata>(x.MetaData, _jsonSerializerSettings))
            )
            .ToList();
        return new Inventory(element, inventory.Size, inventory.Id, items);
    }


    public static InventoryData CreateData(Inventory inventory)
    {
        var data = new InventoryData
        {
            Size = inventory.Size,
            Id = inventory.Id,
            InventoryItems = MapItems(inventory.Items),
        };
        return data;
    }

    private static List<InventoryItemData> MapItems(IReadOnlyList<Item> items)
    {
        var itemsData = items.Select(item => new InventoryItemData
        {
            Id = item.Id,
            ItemId = item.ItemId,
            Number = item.Number,
            MetaData = JsonConvert.SerializeObject(item.MetaData, Formatting.None),
        }).ToList();

        return itemsData;
    }

}
