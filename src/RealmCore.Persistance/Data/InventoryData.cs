namespace RealmCore.Persistance.Data;

public sealed class InventoryData
{
    public int Id { get; set; }
    public decimal Size { get; set; }

    public ICollection<InventoryItemData> InventoryItems = new List<InventoryItemData>();
    public ICollection<UserInventoryData> UserInventories = new List<UserInventoryData>();
    public ICollection<VehicleInventoryData> VehicleInventories = new List<VehicleInventoryData>();
}
