﻿namespace RealmCore.Server.Modules.Inventories;

public class Item : IEquatable<Item>, IEquatable<Metadata>
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
    public string Name { get; init; }
    public decimal Size { get; init; }
    public ItemAction AvailableActions { get; init; }
    private readonly Metadata _metadata;
    public Metadata MetaData
    {
        get
        {
            lock (_lock)
                return new Metadata(_metadata);
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

    public event Action<Item, uint, uint>? NumberChanged;
    public event Action<Item, string>? MetadataChanged;
    public event Action<Item, string>? MetadataRemoved;

    public Item(Item item)
    {
        ItemId = item.ItemId;
        Size = item.Size;
        Name = item.Name;
        _number = item.Number;
        _metadata = new Metadata(item.MetaData);
        AvailableActions = item.AvailableActions;
    }

    internal Item(ItemsCollection itemsCollection, uint itemId, uint number, Metadata? metaData = null, string? id = null)
    {
        var itemsCollectionItem = itemsCollection.Get(itemId);
        AvailableActions = itemsCollectionItem.AvailableActions;
        Size = itemsCollectionItem.Size;
        Name = itemsCollectionItem.Name;
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
        lock (_lock)
        {
            if (value is string or double or int)
            {
                _metadata[key] = value;
                MetadataChanged?.Invoke(this, key);
                return true;
            }
            return false;
        }
    }

    public bool RemoveMetadata(string key)
    {
        lock (_lock)
        {
            if (_metadata.ContainsKey(key))
            {
                _metadata.Remove(key);
                MetadataRemoved?.Invoke(this, key);
                return true;
            }
            return false;
        }
    }

    public void ChangeMetadata<T>(string key, Func<T, T> callback)
    {
        lock (_lock)
        {
            if (_metadata.ContainsKey(key))
            {
                var value = callback((T)_metadata[key]) ?? throw new NullReferenceException("Callback result can not be null");
                _metadata[key] = value;
                MetadataChanged?.Invoke(this, key);
            }
        }
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

    public override string ToString() => Name;

    public bool Equals(Item? other)
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

    public bool Equals(Metadata? other)
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

    public static InventoryItemData CreateData(Item item)
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
