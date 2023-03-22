namespace Realm.Persistance.Data;

public sealed class Inventory
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal Size { get; set; }

    public ICollection<InventoryItem> InventoryItems = new List<InventoryItem>();
}
