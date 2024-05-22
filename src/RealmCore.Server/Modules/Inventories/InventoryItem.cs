namespace RealmCore.Server.Modules.Inventories;

public class InventoryItem : IEquatable<InventoryItem>, IEquatable<ItemMetadata>
{
    private readonly object _lock = new();
    public string Id { get; internal set; }
    public string LocalId { get; } = Guid.NewGuid().ToString();
    public uint ItemId { get; private set; }

    private uint _number = 0;
    public uint Number
    {
        get
        {
            lock (_lock)
                return _number;
        }
        set
        {
            if (value == 0)
                throw new ArgumentException(nameof(value));

            uint old;

            lock (_lock)
            {
                old = _number;
                _number = value;
            }

            if (old != value)
                NumberChanged?.Invoke(this, old, value);
        }
    }

    public decimal Size { get; init; }
    public ItemAction AvailableActions { get; init; }
    private readonly ItemMetadata _metadata;
    public ItemMetadata MetaData
    {
        get
        {
            lock (_lock)
                return new ItemMetadata(_metadata);
        }
    }

    public List<string> MetaDataKeys
    {
        get
        {
            lock (_lock)
                return _metadata.Keys.ToList();
        }
    }

    public event Action<InventoryItem, uint, uint>? NumberChanged;
    public event Action<InventoryItem, string>? MetadataChanged;
    public event Action<InventoryItem, string>? MetadataRemoved;

    public InventoryItem(InventoryItem item)
    {
        Id = Guid.NewGuid().ToString();
        ItemId = item.ItemId;
        Size = item.Size;
        _number = item.Number;
        _metadata = new ItemMetadata(item.MetaData);
        AvailableActions = item.AvailableActions;
    }

    internal InventoryItem(ItemsCollection itemsCollection, uint itemId, uint number, ItemMetadata? metaData = null, string? id = null)
    {
        var itemsCollectionItem = itemsCollection.Get(itemId);
        AvailableActions = itemsCollectionItem.AvailableActions;
        Size = itemsCollectionItem.Size;
        ItemId = itemsCollectionItem.Id;
        Id = id ?? Guid.NewGuid().ToString();
        _number = number;
        if (metaData != null)
        {
            _metadata = metaData;
        }
        else
        {
            _metadata = [];
        }
    }

    public bool SetMetadata(string key, object value)
    {
        if (value is not (string or double or int))
            return false;

        lock (_lock)
        {
            _metadata[key] = value;
        }

        MetadataChanged?.Invoke(this, key);
        return true;
    }

    public bool RemoveMetadata(string key)
    {
        bool removed = false;
        lock (_lock)
        {
            if (_metadata.ContainsKey(key))
            {
                _metadata.Remove(key);
                removed = true;
            }
        }

        if(removed)
            MetadataRemoved?.Invoke(this, key);

        return removed;
    }

    public void ChangeMetadata<T>(string key, Func<T, T> callback)
    {
        bool changed = false;
        lock (_lock)
        {
            if (_metadata.TryGetValue(key, out object? metadataValue))
            {
                var newValue = callback((T)metadataValue);
                if (newValue == null)
                {
                    _metadata.Remove(key);
                }
                else
                    _metadata[key] = newValue;
                changed = true;
            }
        }

        if(changed)
            MetadataChanged?.Invoke(this, key);
    }

    public object? GetMetadata(string key)
    {
        lock (_lock)
        {
            if (_metadata.TryGetValue(key, out var value))
                return value;
            return null;
        }
    }

    public bool TryGetMetadata<T>(string key, out T? value)
    {
        lock (_lock)
        {
            if (_metadata.TryGetValue(key, out var outValue))
            {
                value = (T)outValue;
                return true;
            }
            value = default;
            return false;
        }
    }

    public T? GetMetadata<T>(string key)
    {
        var value = GetMetadata(key);
        if (value == null)
        {
            return default;
        }
        return (T)Convert.ChangeType(value, typeof(T));
    }

    public object? GetMetadata(string key, Type type)
    {
        var value = GetMetadata(key);
        if (value == null)
        {
            return null;
        }
        return Convert.ChangeType(value, type);
    }

    public bool TryEnumValue<TEnum>(string key, out TEnum outValue) where TEnum: struct
    {
        string? metadata = (string?)GetMetadata(key);
        if (metadata == null)
        {
            outValue = default;
            return false;
        }

        return Enum.TryParse(metadata, out outValue);
    }

    public bool HasMetadata(string key)
    {
        lock (_lock)
            return _metadata.ContainsKey(key);
    }

    public bool Equals(InventoryItem? other)
    {
        // TODO: Make sure it really works.

        if (other == this)
            return true;

        if (other == null)
            return false;

        if (ItemId != other.ItemId)
            return false;

        return Equals(other._metadata);
    }

    public bool Equals(ItemMetadata? other)
    {
        if (other == null)
            return false;

        lock (_lock)
        {
            if (_metadata.Count != other.Count)
                return false;

            foreach (var key in _metadata.Keys)
            {
                if (!_metadata[key].Equals(other[key]))
                {
                    return false;
                }
            }
            return true;
        }
    }

    public static InventoryItemData CreateData(InventoryItem item)
    {
        return new InventoryItemData
        {
            Id = item.Id,
            ItemId = item.ItemId,
            Number = item.Number,
            MetaData = JsonConvert.SerializeObject(item.MetaData, Formatting.None),
        };
    }
}
