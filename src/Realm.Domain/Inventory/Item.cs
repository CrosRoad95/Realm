using static Realm.Domain.Registries.ItemRegistryEntry;

namespace Realm.Domain.Inventory;

public class Item : IEquatable<Item>, IEquatable<Dictionary<string, object>>
{
    public string LocalId { get; } = Guid.NewGuid().ToString();
    public uint ItemId { get; private set; }

    private uint _number = 0;
    public uint Number { get => _number; set
        {
            var old = _number;
            _number = value;
            if (old != 0)
            {
                NumberChanged?.Invoke(this, old, _number);
            }
        }
    }
    public string Name { get; init; }
    public uint Size { get; init; }
    public ItemRegistryEntry.ItemAction AvailiableActions { get; init; }
    public Dictionary<string, object> MetaData { get; init; }
    
    public event Action<Item, uint, uint>? NumberChanged;
    public event Action<Item>? Changed;

    internal Item(ItemRegistryEntry itemRegistryEntry, uint number, Dictionary<string, object>? metaData = null)
    {
        AvailiableActions = itemRegistryEntry.AvailiableActions;
        Size = itemRegistryEntry.Size;
        Name = itemRegistryEntry.Name;
        ItemId = itemRegistryEntry.Id;
        _number = number;
        if (metaData != null)
        {
            MetaData = metaData;
        }
        else
        {
            MetaData = new();
        }
    }

    public bool SetMetadata(string key, object value)
    {
        if (value is string or double or int)
        {
            MetaData[key] = value;
            Changed?.Invoke(this);
            return true;
        }
        return false;
    }

    public object? GetMetadata(string key)
    {
        if (MetaData.TryGetValue(key, out var value))
            return value;
        return null;
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

        return Equals(other.MetaData);
    }

    public bool Equals(Dictionary<string, object>? other)
    {
        if (other == null)
            return false;

        if (MetaData == other || MetaData.Count == other.Count)
            return true;

        foreach (var key in MetaData.Keys)
        {
            if (MetaData[key].Equals(other[key]))
            {
                return true;
            }
        }
        return false;
    }
}
