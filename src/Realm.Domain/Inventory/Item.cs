namespace Realm.Domain.Inventory;

public class Item : IEquatable<Item>, IEquatable<Dictionary<string, object>>
{
    private readonly ItemRegistryEntry _itemRegistryEntry;
    private uint _number = 0;

    internal string? Id { get; set; }
    public string LocalId { get; } = Guid.NewGuid().ToString();
    public uint ItemId { get; private set; }
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
    public string Name => _itemRegistryEntry.Name;
    public uint Size => _itemRegistryEntry.Size;
    public ItemRegistryEntry.ItemAction AvailiableActions => _itemRegistryEntry.AvailiableActions;

    private Dictionary<string, object> _metadata;

    public Dictionary<string, object> MetaData { get => _metadata; set { _metadata = value; } }

    public event Action<Item, uint, uint>? NumberChanged;
    public event Action<Item>? Changed;

    internal Item(ItemRegistryEntry itemRegistryEntry)
    {
        _itemRegistryEntry = itemRegistryEntry;
        ItemId = itemRegistryEntry.Id;
        _metadata = new();
    }

    internal Item(InventoryItem inventoryItem, ItemRegistryEntry itemRegistryEntry)
    {
        _itemRegistryEntry = itemRegistryEntry;
        Id = inventoryItem.Id;
        ItemId = inventoryItem.ItemId;
        Number = inventoryItem.Number;
        var metadata = JsonConvert.DeserializeObject<Dictionary<string, object>>(inventoryItem.MetaData);
        if (metadata != null)
        {
            _metadata = metadata;
        }
        else
        {
            _metadata = new();
        }
    }

    public bool SetMetadata(string key, object value)
    {
        if (value is string or double or int)
        {
            _metadata[key] = value;
            Changed?.Invoke(this);
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
