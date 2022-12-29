namespace Realm.Persistance.Data;

public class InventoryItem
{
    public string Id { get; set; }
    public uint ItemId { get; set; }
    public uint Number { get; set; }
    public string InventoryId { get; set; }
    public string MetaData { get; set; }

    public Inventory? Inventory { get; set; }
}
