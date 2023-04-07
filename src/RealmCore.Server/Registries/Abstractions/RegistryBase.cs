namespace RealmCore.Server.Registries.Abstractions;

public abstract class RegistryBase<TEntry> where TEntry : RegistryEntryBase
{
    protected readonly Dictionary<int, TEntry> _entries = new();

    public TEntry Get(int id)
    {
        return _entries[id];
    }

    public void Add(int id, TEntry entry)
    {
        if (_entries.ContainsKey(id))
        {
            throw new Exception("Entry of id '" + id + "' already exists;.");
        }
        entry.Id = id;
        _entries[id] = entry;
    }
}
