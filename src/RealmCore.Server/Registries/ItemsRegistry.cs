using RealmCore.Server.Registries.Abstractions;

namespace RealmCore.Server.Registries;

public class ItemsRegistry : RegistryBase<uint, ItemRegistryEntry>
{
    private readonly Dictionary<uint, ItemRegistryEntry> _itemRegistryEntries = new();

    public Func<InventoryComponent, Item, ItemAction, Task> UseCallback { get; set; } = default!;

    public ItemsRegistry()
    {
    }
}
