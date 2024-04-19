namespace RealmCore.Server.Modules.Inventories;

public class ItemsCollection : CollectionBase<uint, ItemsCollectionItem>
{
    public decimal CalculateSize(uint itemId, uint number = 1)
    {
        var itemsCollectionItem = Get(itemId);
        return itemsCollectionItem.Size * number;
    }

    public decimal CalculateSize(Dictionary<uint, uint> items) => items.Sum(x => CalculateSize(x.Key, x.Value));
}
