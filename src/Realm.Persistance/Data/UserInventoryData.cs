namespace Realm.Persistance.Data;

public class UserInventoryData
{
    public int UserId { get; set; }
    public int InventoryId { get; set; }

    public UserData? User { get; set; }
    public InventoryData? Inventory { get; set; }
}
