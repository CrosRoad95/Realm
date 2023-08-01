namespace RealmCore.Server.Concepts;

public class Item : IEquatable<Item>, IEquatable<Dictionary<string, object>>
{
    private readonly object _lock = new();
    public string Id { get; internal set; } = Guid.NewGuid().ToString();
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

            if(old != value)
                NumberChanged?.Invoke(this, old, value);
        }
    }
    public string Name { get; init; }
    public decimal Size { get; init; }
    public ItemAction AvailableActions { get; init; }
    private readonly Dictionary<string, object> _metaData;
    public IReadOnlyDictionary<string, object> MetaData
    {
        get
        {
            lock (_lock)
                return new Dictionary<string, object>(_metaData);
        }
    }

    public List<string> MetaDataKeys
    {
        get
        {
            lock (_lock)
                return _metaData.Keys.ToList();
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
        _metaData = new Dictionary<string, object>(item.MetaData);
        AvailableActions = item.AvailableActions;
    }

    internal Item(ItemsRegistry itemsRegistry, uint itemId, uint number, Dictionary<string, object>? metaData = null)
    {
        var itemRegistryEntry = itemsRegistry.Get(itemId);
        AvailableActions = itemRegistryEntry.AvailableActions;
        Size = itemRegistryEntry.Size;
        Name = itemRegistryEntry.Name;
        ItemId = itemRegistryEntry.Id;
        _number = number;
        if (metaData != null)
        {
            _metaData = metaData;
        }
        else
        {
            _metaData = new();
        }
    }

    public bool SetMetadata(string key, object value)
    {
        lock (_lock)
        {
            if (value is string or double or int)
            {
                _metaData[key] = value;
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
            if (_metaData.ContainsKey(key))
            {
                _metaData.Remove(key);
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
            if (_metaData.ContainsKey(key))
            {
                var value = callback((T)_metaData[key]);
                if (value == null)
                    throw new NullReferenceException("Callback result can not be null");
                _metaData[key] = value;
                MetadataChanged?.Invoke(this, key);
            }
        }
    }

    public object? GetMetadata(string key)
    {
        lock (_lock)
        {
            if (_metaData.TryGetValue(key, out var value))
                return value;
            return null;
        }
    }
    
    public bool TryGetMetadata<T>(string key, out T? value)
    {
        lock (_lock)
        {
            if (_metaData.TryGetValue(key, out var outValue))
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

    public bool HasMetadata(string key)
    {
        lock (_lock)
            return _metaData.ContainsKey(key);
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

        return Equals(other._metaData);
    }

    public bool Equals(Dictionary<string, object>? other)
    {
        if (other == null)
            return false;

        lock (_lock)
        {
            if (_metaData.Count != other.Count)
                return false;

            foreach (var key in _metaData.Keys)
            {
                if (!_metaData[key].Equals(other[key]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
