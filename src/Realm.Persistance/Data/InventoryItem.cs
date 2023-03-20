namespace Realm.Persistance.Data;

public sealed class InventoryItem
{
    public string Id { get; set; }
    public uint ItemId { get; set; }
    public uint Number { get; set; }
    public int InventoryId { get; set; }
    public string MetaData { get; set; }

    public Inventory? Inventory { get; set; }
}
