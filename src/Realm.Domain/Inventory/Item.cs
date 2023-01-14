namespace Realm.Domain.Inventory;

public class Item
{
    public string? Id { get; set; }
    public uint ItemId { get; private set; }
    public uint Number { get; set; } = 1;

    public string Name => InventoryComponent!.GetItemRegistryEntry(ItemId).Name;
    public ItemRegistryEntry.ItemAction AvailiableActions => InventoryComponent!.GetItemRegistryEntry(ItemId).AvailiableActions;
    public uint Size { get; set; } = 1;

    private Dictionary<string, object> _metadata;

    public Dictionary<string, object> MetaData { get => _metadata; set { _metadata = value; } }
    public InventoryComponent? InventoryComponent { get; set; }

    public event Action<Item>? Changed;

    public Item(ItemRegistryEntry itemRegistryEntry, uint number = 1)
    {
        ItemId = itemRegistryEntry.Id;
        Size = itemRegistryEntry.Size;
        Number = number;
        _metadata = new();
    }
    
    public Item(InventoryItem inventoryItem)
    {
        ItemId = inventoryItem.ItemId;
        Number = inventoryItem.Number;
        try
        {
            var metadata = JsonConvert.DeserializeObject<Dictionary<string, object>>(inventoryItem.MetaData);
            if(metadata != null)
            {
                _metadata = metadata;
            }
            else
            {
                _metadata = new();
            }
        }
        finally
        {
            // TODO: catch exception
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
}
