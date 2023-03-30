namespace Realm.Domain.Registries;

public class ItemsRegistry
{
    private readonly Dictionary<uint, ItemRegistryEntry> _itemRegistryEntries = new();

    public Func<InventoryComponent, Item, ItemAction, Task> UseCallback { get; set; } = default!;

    public ItemsRegistry()
    {
    }

    public ItemRegistryEntry Get(uint id)
    {
        return _itemRegistryEntries[id];
    }

    public void AddItem(uint id, ItemRegistryEntry itemRegistryEntry)
    {
        if (_itemRegistryEntries.ContainsKey(id))
        {
            throw new Exception("Item of id '" + id + "' already exists;.");
        }
        itemRegistryEntry.Id = id;
        _itemRegistryEntries[id] = itemRegistryEntry;
    }
}
