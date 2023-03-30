namespace Realm.Domain.Inventory;

public class Item : IEquatable<Item>, IEquatable<Dictionary<string, object>>
{
    private readonly ReaderWriterLockSlim _lock = new();
    public string Id { get; internal set; } = Guid.NewGuid().ToString();
    public string LocalId { get; } = Guid.NewGuid().ToString();
    public uint ItemId { get; private set; }

    private uint _number = 0;
    public uint Number
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _number;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
        set
        {
            if (value == 0)
                throw new ArgumentException(nameof(value));

            _lock.EnterWriteLock();
            var old = _number;
            _number = value;
            _lock.ExitWriteLock();

            NumberChanged?.Invoke(this, old, value);
        }
    }
    public string Name { get; init; }
    public decimal Size { get; init; }
    public ItemAction AvailiableActions { get; init; }
    private readonly Dictionary<string, object> _metaData;
    public IReadOnlyDictionary<string, object> MetaData
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return new Dictionary<string, object>(_metaData);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public event Action<Item, uint, uint>? NumberChanged;
    public event Action<Item>? Changed;

    internal Item(ItemsRegistry itemsRegistry, uint itemId, uint number, Dictionary<string, object>? metaData = null)
    {
        var itemRegistryEntry = itemsRegistry.Get(itemId);
        AvailiableActions = itemRegistryEntry.AvailiableActions;
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
        _lock.EnterWriteLock();
        try
        {
            if (value is string or double or int)
            {
                _metaData[key] = value;
                Changed?.Invoke(this);
                return true;
            }
            return false;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public object? GetMetadata(string key)
    {
        _lock.EnterReadLock();
        try
        {
            if (_metaData.TryGetValue(key, out var value))
                return value;
            return null;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
    

    public bool HasMetadata(string key)
    {
        _lock.EnterReadLock();
        try
        {
            return _metaData.ContainsKey(key);
        }
        finally
        {
            _lock.ExitReadLock();
        }
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

        _lock.EnterReadLock();
        try
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
        finally
        {
            _lock.ExitReadLock();
        }
    }
}
