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
    private readonly ItemsCollection _itemsCollection;

    public Element? Owner { get; }
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

    public Inventory(Element? owner, decimal size, ItemsCollection itemsCollection)
    {
        Owner = owner;
        _size = size;
        _itemsCollection = itemsCollection;
    }

    internal Inventory(Element? owner, decimal size, int id, IEnumerable<InventoryItem> items, ItemsCollection itemsCollection)
    {
        Owner = owner;
        _size = size;
        Id = id;
        _itemsCollection = itemsCollection;
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
        return _items.Any(x => item.Id == x.Id);
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

    public bool TryGetByLocalId(string localId, out InventoryItem inventoryItem)
    {
        using var _ = _lock.BeginRead();
        inventoryItem = _items.FirstOrDefault(x => x.LocalId == localId)!;
        return inventoryItem != null;
    }

    public bool TryGetByItemId(uint itemId, out InventoryItem inventoryItem)
    {
        using var _ = _lock.BeginRead();
        inventoryItem = _items.FirstOrDefault(x => x.ItemId == itemId)!;
        return inventoryItem != null;
    }

    public bool TryGetByIdAndMetadata(uint itemId, ItemMetadata metadata, out InventoryItem item)
    {
        using var _ = _lock.BeginRead();
        item = _items.FirstOrDefault(x => x.ItemId == itemId && x.Equals(metadata))!;
        return item != null;
    }

    public InventoryItem AddItem(uint itemId, ItemMetadata? metadata = null, bool tryStack = true, bool force = false)
    {
        using var _ = _lock.BeginWrite();
        var item = AddItems(itemId, 1, metadata, tryStack, force);
        if (item.Any())
            return item.First();
        return GetSingleItemByIdWithMetadata(itemId, metadata ?? []) ?? throw new InvalidOperationException();
    }

    public IEnumerable<InventoryItem> AddItems(uint itemId, uint number, ItemMetadata? metadata = null, bool tryStack = true, bool force = false)
    {
        if (number <= 0)
            throw new ArgumentOutOfRangeException(nameof(number));

        metadata ??= [];

        var itemsCollectionItem = _itemsCollection.Get(itemId);
        var requiredSpace = Number + _itemsCollection.CalculateSize(itemId, number);
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
            var item = new InventoryItem(_itemsCollection, itemId, number, metadata)
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
        }

        return newItems.AsReadOnly();
    }

    private void HandleItemMetadataChanged(InventoryItem item, string key)
    {
        ItemChanged?.Invoke(this, item);
    }
    
    private void HandleItemNumberChanged(InventoryItem item, uint from, uint to)
    {
        ItemChanged?.Invoke(this, item);
    }

    public bool RemoveItemByItemId(uint id, uint number = 1)
    {
        if (TryGetByItemId(id, out var item))
            return RemoveItem(item.LocalId, number);
        return false;
    }

    public bool RemoveItem(string localId, uint number = 1)
    {
        if (number <= 0)
            return false;

        using var _ = _lock.BeginWrite();
        if (!TryGetByLocalId(localId, out var inventoryItem))
        {
            return false;
        }

        if (inventoryItem.Number > number)
        {
            inventoryItem.Number -= number;
        }
        else if (inventoryItem.Number == number)
        {
            if (!_items.Remove(inventoryItem))
                throw new InvalidOperationException();

            inventoryItem.MetadataChanged -= HandleItemMetadataChanged;
            inventoryItem.NumberChanged -= HandleItemNumberChanged;
            ItemRemoved?.Invoke(this, inventoryItem);
        }
        else
        {
            return false;
        }
        return true;
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

    public bool HasSpaceForItem(uint itemId, uint number = 1)
    {
        return Number + _itemsCollection.CalculateSize(itemId, number) <= Size;
    }
    
    public bool HasSpaceForItems(Dictionary<uint, uint> items)
    {
        return Number + _itemsCollection.CalculateSize(items) <= Size;
    }

    public bool TransferItem(Inventory destination, string localId, uint number, bool tryStack = true, bool force = false)
    {
        if (!TryGetByLocalId(localId, out var inventoryItem))
            return false;

        var itemId = inventoryItem.ItemId;

        if (!force && !destination.HasSpaceForItem(itemId, number))
            return false;

        if (!RemoveItem(localId, number))
            return false;

        destination.AddItems(inventoryItem.ItemId, number, inventoryItem.MetaData, tryStack, force);

        return true;
    }

    public void Clear()
    {
        var view = Items.ToArray();
        foreach (var item in view)
            RemoveItem(item.LocalId, item.Number);
    }

    private static readonly JsonSerializerSettings _jsonSerializerSettings = new()
    {
        Converters = new List<JsonConverter> { DoubleConverter.Instance }
    };

    public static Inventory CreateFromData(Element? element, InventoryData inventory, ItemsCollection itemsCollection)
    {
        var items = inventory.InventoryItems
            .Select(x =>
                new InventoryItem(itemsCollection, x.ItemId, x.Number, JsonConvert.DeserializeObject<ItemMetadata>(x.MetaData, _jsonSerializerSettings), x.Id)
            )
            .ToList();
        return new Inventory(element, inventory.Size, inventory.Id, items, itemsCollection);
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
