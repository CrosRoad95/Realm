namespace Realm.Domain.Inventory;

public class Item
{
    public string UniqueId { get; private set; }
    public int Id { get; set; }
    public string Name { get; set; }
    public int Size { get; set; }

    private Dictionary<string, object> _metadata;

    public Dictionary<string, object> MetaData { get => _metadata; set { _metadata = value; } }

    public Item(int id, string name, int size)
    {
        if (size < 0)
            throw new ArgumentOutOfRangeException(nameof(size));
        UniqueId = Guid.NewGuid().ToString();
        Id = id;
        Name = name;
        Size = size;
        _metadata = new();
    }

    public bool SetMetadata(string key, object value)
    {
        if (value is string or double or int)
        {
            _metadata[key] = value;
            return true;
        }
        return false;
    }

    public object? GetMetadata(string key)
    {
        if (_metadata.TryGetValue(key, out var value))
            return value;
        return null;
    }

    public override string ToString() => Name;
}
