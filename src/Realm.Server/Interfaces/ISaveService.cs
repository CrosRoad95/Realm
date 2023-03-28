namespace Realm.Server.Interfaces;

public interface ISaveService
{
    Task<bool> Save(Entity entity);
    Task Commit();
    
    internal Task<int> SaveNewPlayerInventory(InventoryComponent inventoryComponent, int userId);
}
