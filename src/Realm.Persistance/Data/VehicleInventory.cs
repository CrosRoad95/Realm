namespace Realm.Persistance.Data;

public class VehicleInventory
{
    public int VehicleId { get; set; }
    public int InventoryId { get; set; }

    public Vehicle? Vehicle { get; set; }
    public Inventory? Inventory { get; set; }
}
