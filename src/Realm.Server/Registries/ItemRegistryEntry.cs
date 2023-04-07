namespace Realm.Server.Registries;

public class ItemRegistryEntry
{
    public uint Id { get; set; }
    public decimal Size { get; set; }
    public string Name { get; set; }
    public uint StackSize { get; set; } = 1; // 1 = not stackable
    public ItemAction AvailiableActions { get; set; }
}
