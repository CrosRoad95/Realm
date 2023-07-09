namespace RealmCore.Persistance.Data;

public class VehicleInventoryData
{
    public int VehicleId { get; set; }
    public int InventoryId { get; set; }

    public VehicleData? Vehicle { get; set; }
    public InventoryData? Inventory { get; set; }
}
