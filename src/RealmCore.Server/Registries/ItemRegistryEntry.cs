namespace RealmCore.Server.Registries;

public class ItemRegistryEntry : RegistryEntryBase<uint>
{
    public decimal Size { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public uint StackSize { get; set; } = 1; // 1 = not stackable
    public ItemAction AvailiableActions { get; set; }
}
