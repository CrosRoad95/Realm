namespace RealmCore.Server.Modules.Inventories;

public sealed class InventoryService
{
    private readonly ItemsCollection _itemsCollection;

    public InventoryService(ItemsCollection itemsCollection)
    {
        _itemsCollection = itemsCollection;
    }

    public InventoryItem CreateInventoryItem(uint itemId, uint number, ItemMetadata? itemMetadata = null)
    {
        return new InventoryItem(_itemsCollection, itemId, number, itemMetadata);
    }
}
