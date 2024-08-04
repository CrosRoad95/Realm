namespace RealmCore.Persistence.Data;

public sealed class InventoryData
{
    public int Id { get; set; }
    public decimal Size { get; set; }

    public ICollection<InventoryItemData> InventoryItems { get; set; } = new List<InventoryItemData>();
    public ICollection<UserInventoryData> UserInventories { get; set; } = new List<UserInventoryData>();
    public ICollection<VehicleInventoryData> VehicleInventories { get; set; } = new List<VehicleInventoryData>();
}

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
