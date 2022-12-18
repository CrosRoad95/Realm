namespace Realm.Domain.Inventory;

public class Item
{
    [ScriptMember("uniqueId", ScriptAccess.ReadOnly)]
    public string UniqueId { get; private set; }
    [ScriptMember("id")]
    public int Id { get; set; }
    [ScriptMember("name")]
    public string Name { get; set; }
    [ScriptMember("size")]
    public int Size { get; set; }

    private Dictionary<string, object> _metadata;

    [NoScriptAccess]
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

    [ScriptMember("setMetadata")]
    public bool SetMetadata(string key, object value)
    {
        if (value is string or double or int)
        {
            _metadata[key] = value;
            return true;
        }
        return false;
    }

    [ScriptMember("getMetadata")]
    public object? GetMetadata(string key)
    {
        if (_metadata.TryGetValue(key, out var value))
            return value;
        return null;
    }

    [ScriptMember("toString")]
    public override string ToString() => Name;
}
