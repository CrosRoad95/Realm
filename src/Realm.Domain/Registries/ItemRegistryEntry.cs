namespace Realm.Domain.Registries;

public class ItemRegistryEntry
{
    public uint Id { get; set; }
    public uint Size { get; set; }
    public string Name { get; set; }
    public uint StackSize { get; set; } = 1; // 1 = not stackable
}
