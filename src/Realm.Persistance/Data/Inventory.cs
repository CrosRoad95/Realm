namespace Realm.Persistance.Data;

public class Inventory
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public uint Size { get; set; }

    public ICollection<InventoryItem> InventoryItems = new List<InventoryItem>();

    public virtual User? User { get; set; }
}
