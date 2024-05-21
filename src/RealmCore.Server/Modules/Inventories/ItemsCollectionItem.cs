namespace RealmCore.Server.Modules.Inventories;

public class ItemsCollectionItem : CollectionItemBase<uint>
{
    public decimal Size { get; set; }
    public uint StackSize { get; set; } = 1; // 1 = not stackable
    public ItemAction AvailableActions { get; set; }
}
