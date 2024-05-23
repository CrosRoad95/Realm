namespace RealmCore.Persistence.Data;

public sealed class InventoryData
{
    public int Id { get; set; }
    public decimal Size { get; set; }

    public ICollection<InventoryItemData> InventoryItems { get; set; } = new List<InventoryItemData>();
    public ICollection<UserInventoryData> UserInventories { get; set; } = new List<UserInventoryData>();
    public ICollection<VehicleInventoryData> VehicleInventories { get; set; } = new List<VehicleInventoryData>();
}
