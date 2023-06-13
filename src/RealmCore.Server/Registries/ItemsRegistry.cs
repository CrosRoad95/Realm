namespace RealmCore.Server.Registries;

public class ItemsRegistry : RegistryBase<uint, ItemRegistryEntry>
{
    public Func<InventoryComponent, Item, ItemAction, Task> UseCallback { get; set; } = default!;

    public ItemsRegistry()
    {
    }
}
