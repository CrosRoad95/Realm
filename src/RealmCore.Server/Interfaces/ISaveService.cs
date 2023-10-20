namespace RealmCore.Server.Interfaces;

public interface ISaveService
{
    event Action<Entity>? EntitySaved;

    Task<bool> BeginSave(Entity entity);
    Task Commit();
    Task<bool> Save(Entity entity);
    internal Task<int> SaveNewPlayerInventory(InventoryComponent inventoryComponent, int userId);
    internal Task<int> SaveNewVehicleInventory(InventoryComponent inventoryComponent, int vehicleId);
}
