namespace RealmCore.Persistance.Data;

public sealed class InventoryItemData
{
    public string Id { get; set; }
    public uint ItemId { get; set; }
    public uint Number { get; set; }
    public int InventoryId { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public string MetaData { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public InventoryData? Inventory { get; set; }
}
