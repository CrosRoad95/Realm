namespace Realm.Server.Concepts.Inventory;

public class PlayerItem : ISerializable
{
    [ScriptMember("uniqueId")]
    public string UniqueId { get; [NoScriptAccess] set; }
    [ScriptMember("id")]
    public int Id { get; set; }
    [ScriptMember("name")]
    public string Name { get; set; }
    [ScriptMember("size")]
    public int Size { get; set; }

    private Dictionary<string, object> _metadata;

    [NoScriptAccess]
    public Dictionary<string, object> MetaData { get => _metadata; set { _metadata = value; } }

    public PlayerItem(int id, string name, int size)
    {
        if (size < 0)
            throw new ArgumentOutOfRangeException(nameof(size));
        UniqueId = Guid.NewGuid().ToString();
        Id = id;
        Name = name;
        Size = size;
        _metadata = new();
    }

    [JsonConstructor]
    public PlayerItem(string uniqueId, int id, string name, int size, Dictionary<string, object> metadata)
    {
        if (size < 0)
            throw new ArgumentOutOfRangeException(nameof(size));
        UniqueId = uniqueId;
        Id = id;
        Name = name;
        Size = size;
        _metadata = metadata ?? new();
    }

    public PlayerItem(SerializationInfo info, StreamingContext context)
    {
        UniqueId = info.GetString("UniqueId") ?? throw new SerializationException();
        Id = info.GetInt32("Id");
        Name = info.GetString("Name") ?? throw new SerializationException();
        Size = info.GetInt32("Size");
        _metadata = (Dictionary<string, object>?)info.GetValue("MetaData", typeof(Dictionary<string, object>)) ?? throw new SerializationException();
    }

    [NoScriptAccess]
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("UniqueId", UniqueId);
        info.AddValue("Id", Id);
        info.AddValue("Name", Name);
        info.AddValue("Size", Size);
        info.AddValue("Metadata", _metadata);
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
