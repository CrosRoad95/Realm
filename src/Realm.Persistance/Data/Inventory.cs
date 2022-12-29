namespace Realm.Persistance.Data;

public class Inventory
{
    public string Id { get; set; }
    public uint Size { get; set; }

    public ICollection<InventoryItem> InventoryItems = new List<InventoryItem>();
}
