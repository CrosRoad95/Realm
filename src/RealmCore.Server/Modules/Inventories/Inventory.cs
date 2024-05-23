namespace RealmCore.Server.Modules.Inventories;

public class Inventory
{
    private readonly List<InventoryItem> _items = [];
    private readonly ReaderWriterLockSlimScoped _lock = new(LockRecursionPolicy.SupportsRecursion);
    public event Action<Inventory, InventoryItem>? ItemAdded;
    public event Action<Inventory, InventoryItem>? ItemRemoved;
    public event Action<Inventory, InventoryItem>? ItemChanged;
    public event Action<Inventory, InventoryItem, ItemAction>? ItemUsed;
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
            using var _ = _lock.BeginRead();
            return _items.Sum(x => x.Size * x.Number);
        }
    }

    public IReadOnlyList<InventoryItem> Items
    {
        get
        {
            using var _ = _lock.BeginRead();
            return new List<InventoryItem>(_items);
        }
    }

    public Inventory(Element owner, decimal size)
    {
        Owner = owner;
        _size = size;
    }

    internal Inventory(Element owner, decimal size, int id, IEnumerable<InventoryItem> items)
    {
        Owner = owner;
        _size = size;
        Id = id;
        _items = items.ToList();
    }

    public bool HasItemById(uint itemId)
    {
        using var _ = _lock.BeginRead();
        return _items.Any(x => x.ItemId == itemId);
    }

    public bool HasItem(Func<InventoryItem, bool> callback)
    {
        using var _ = _lock.BeginRead();
        return _items.Any(callback);
    }

    public bool HasItem(InventoryItem item)
    {
        using var _ = _lock.BeginRead();
        return _items.Contains(item);
    }

    public int SumItemsById(uint itemId)
    {
        using var _ = _lock.BeginRead();
        return _items.Count(x => x.ItemId == itemId);
    }

    public int SumItemsNumberById(uint itemId)
    {
        using var _ = _lock.BeginRead();
        return (int)_items.Where(x => x.ItemId == itemId).Sum(x => x.Number);
    }

    public IReadOnlyList<InventoryItem> GetItemsById(uint itemId)
    {
        using var _ = _lock.BeginRead();
        return new List<InventoryItem>(_items.Where(x => x.ItemId == itemId));
    }

    public IReadOnlyList<InventoryItem> GetItemsByIdWithMetadata(uint itemId, string key, object? metadata)
    {
        using var _ = _lock.BeginRead();
        return new List<InventoryItem>(_items.Where(x => x.ItemId == itemId && (x.GetMetadata(key)?.Equals(metadata) ?? false)));
    }

    public InventoryItem? GetSingleItemByIdWithMetadata(uint itemId, ItemMetadata metadata)
    {
        using var _ = _lock.BeginRead();
        return _items.Where(x => x.ItemId == itemId && x.Equals(metadata)).FirstOrDefault();
    }

    public bool HasItemWithMetadata(uint itemId, string key, object? metadata)
    {
        using var _ = _lock.BeginRead();
        return _items.Any(x => x.ItemId == itemId && (x.GetMetadata(key)?.Equals(metadata) ?? false));
    }

    public bool TryGetByLocalId(string localId, out InventoryItem item)
    {
        using var _ = _lock.BeginRead();
        item = _items.FirstOrDefault(x => x.LocalId == localId)!;
        return item != null;
    }

    public bool TryGetByItemId(uint itemId, out InventoryItem item)
    {
        using var _ = _lock.BeginRead();
        item = _items.FirstOrDefault(x => x.ItemId == itemId)!;
        return item != null;
    }

    public bool TryGetByIdAndMetadata(uint itemId, ItemMetadata metadata, out InventoryItem item)
    {
        using var _ = _lock.BeginRead();
        item = _items.FirstOrDefault(x => x.ItemId == itemId && x.Equals(metadata))!;
        return item != null;
    }

    public InventoryItem AddSingleItem(ItemsCollection itemsCollection, uint itemId, ItemMetadata? metadata = null, bool tryStack = true, bool force = false)
    {
        using var _ = _lock.BeginWrite();
        var item = AddItem(itemsCollection, itemId, 1, metadata, tryStack, force);
        if (item.Any())
            return item.First();
        return GetSingleItemByIdWithMetadata(itemId, metadata ?? []) ?? throw new InvalidOperationException();
    }

    public IEnumerable<InventoryItem> AddItem(ItemsCollection itemsCollection, uint itemId, uint number = 1, ItemMetadata? metadata = null, bool tryStack = true, bool force = false)
    {
        if (number <= 0)
            throw new ArgumentOutOfRangeException(nameof(number));

        metadata ??= [];

        var itemsCollectionItem = itemsCollection.Get(itemId);
        var requiredSpace = Number + itemsCollection.CalculateSize(itemId, number);
        if (requiredSpace > Size && !force)
            throw new InventoryNotEnoughSpaceException(Size, requiredSpace);

        using var _ = _lock.BeginWrite();
        List<InventoryItem> newItems = [];
        if (tryStack)
        {
            foreach (var item in _items.Where(x => x.ItemId == itemId))
            {
                if (!item.Equals(metadata))
                    continue;

                var added = Math.Min(number, itemsCollectionItem.StackSize - item.Number);
                item.Number += added;
                number -= added;
            }
        }

        while (number > 0)
        {
            var thisItemNumber = Math.Min(number, itemsCollectionItem.StackSize);
            number -= thisItemNumber;
            var item = new InventoryItem(itemsCollection, itemId, number, metadata)
            {
                Number = thisItemNumber
            };
            newItems.Add(item);
        }

        foreach (var newItem in newItems)
        {
            newItem.MetadataChanged += HandleItemMetadataChanged;
            newItem.NumberChanged += HandleItemNumberChanged;
            _items.Add(newItem);
            ItemAdded?.Invoke(this, newItem);
            ItemChanged?.Invoke(this, newItem);
        }

        return newItems.AsReadOnly();
    }

    public void AddItem(ItemsCollection itemsCollection, InventoryItem newItem, bool tryStack = true, bool force = false)
    {
        AddItem(itemsCollection, newItem.ItemId, newItem.Number, newItem.MetaData, tryStack, force);
    }

    public void AddItems(ItemsCollection itemsCollection, IEnumerable<InventoryItem> newItems, bool force = false)
    {
        using var _ = _lock.BeginWrite();

        if (!force)
        {
            decimal requiredSpace = 0;
            foreach (var newItem in newItems)
            {
                requiredSpace += Number + itemsCollection.CalculateSize(newItem.ItemId, newItem.Number);
            }

            if (requiredSpace > Size)
                throw new InventoryNotEnoughSpaceException(Size, requiredSpace);
        }

        foreach (var newItem in newItems)
        {
            newItem.MetadataChanged += HandleItemMetadataChanged;
            newItem.NumberChanged += HandleItemNumberChanged;
            _items.Add(newItem);
            ItemAdded?.Invoke(this, newItem);
        }
    }

    private void HandleItemMetadataChanged(InventoryItem item, string key)
    {
        ItemChanged?.Invoke(this, item);
    }
    
    private void HandleItemNumberChanged(InventoryItem item, uint from, uint to)
    {
        ItemChanged?.Invoke(this, item);
    }

    public bool RemoveItemStack(InventoryItem item) => RemoveItem(item, item.Number);
    public bool RemoveItemStack(uint id)
    {
        using var _ = _lock.BeginWrite();
        if (TryGetByItemId(id, out var item))
            return RemoveItemStack(item);
        return false;
    }

    public bool RemoveItem(InventoryItem item, uint number = 1)
    {
        using var _ = _lock.BeginWrite();
        return RemoveItem(item, out var _, out var _, number);
    }

    public bool RemoveItem(InventoryItem item, out uint removedNumber, out bool stackRemoved, uint number = 1)
    {
        if (number <= 0)
            throw new ArgumentOutOfRangeException(nameof(number));

        using var _ = _lock.BeginWrite();
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

            item.MetadataChanged -= HandleItemMetadataChanged;
            item.NumberChanged -= HandleItemNumberChanged;
            removedNumber = item.Number;
            stackRemoved = true;
            ItemRemoved?.Invoke(this, item);
        }
        return true;
    }

    public bool RemoveItem(uint id, uint number = 1)
    {
        using var _ = _lock.BeginWrite();

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

    public InventoryItem[] RemoveAndGetItemById(uint id, uint number = 1)
    {
        return RemoveAndGetItemByIdInternal(id, number).ToArray();
    }

    private IEnumerable<InventoryItem> RemoveAndGetItemByIdInternal(uint id, uint number = 1)
    {
        using var _ = _lock.BeginWrite();

        if (SumItemsNumberById(id) >= number && TryGetByItemId(id, out var item))
        {
            var success = RemoveItem(item, out var removedNumber, out bool stackRemoved, number);
            if (item.Number > 0 && !stackRemoved)
            {
                item = new InventoryItem(item)
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

    public bool TryUseItem(InventoryItem item, ItemAction flags)
    {
        using var _ = _lock.BeginRead();

        if (HasItem(item) && (item.AvailableActions & flags) == flags)
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

    public bool HasSpaceForItem(ItemsCollection itemsCollection, uint itemId, uint number = 1)
    {
        return Number + itemsCollection.CalculateSize(itemId, number) <= Size;
    }
    
    public bool HasSpaceForItems(ItemsCollection itemsCollection, Dictionary<uint, uint> items)
    {
        return Number + itemsCollection.CalculateSize(items) <= Size;
    }

    public bool TransferItem(Inventory destination, ItemsCollection itemsCollection, uint itemId, uint number, bool force)
    {
        if (SumItemsNumberById(itemId) >= number || force)
        {
            var removedItems = RemoveAndGetItemById(itemId, number);
            try
            {
                destination.AddItems(itemsCollection, removedItems, force: force);
                return true;
            }
            catch (InventoryNotEnoughSpaceException)
            {
                AddItems(itemsCollection, removedItems);
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

    public static Inventory CreateFromData(Element element, InventoryData inventory, ItemsCollection itemsCollection)
    {
        var items = inventory.InventoryItems
            .Select(x =>
                new InventoryItem(itemsCollection, x.ItemId, x.Number, JsonConvert.DeserializeObject<ItemMetadata>(x.MetaData, _jsonSerializerSettings), x.Id)
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

    private static List<InventoryItemData> MapItems(IReadOnlyList<InventoryItem> items)
    {
        var itemsData = items.Select(item => InventoryItem.CreateData(item)).ToList();

        return itemsData;
    }

}
