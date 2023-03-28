namespace Realm.Persistance.Data;

public class UserInventory
{
    public int UserId { get; set; }
    public int InventoryId { get; set; }

    public User? User { get; set; }
    public Inventory? Inventory { get; set; }
}
