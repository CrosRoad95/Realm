namespace RealmCore.Server.Registries.Abstractions;

public abstract class RegistryBase<TKey, TEntry> where TEntry : RegistryEntryBase<TKey>
    where TKey : unmanaged
{
    protected readonly Dictionary<TKey, TEntry> _entries = [];

    public TEntry Get(TKey id)
    {
        return _entries[id];
    }
    
    public bool HasKey(TKey id)
    {
        return _entries.ContainsKey(id);
    }

    public void Add(TKey id, TEntry entry)
    {
        if (_entries.ContainsKey(id))
        {
            throw new Exception($"Entry of id '{id}' already exists;.");
        }
        entry.Id = id;
        _entries[id] = entry;
    }
}


public abstract class RegistryBase<TEntry> : RegistryBase<int, TEntry>
     where TEntry : RegistryEntryBase<int>
{
}
