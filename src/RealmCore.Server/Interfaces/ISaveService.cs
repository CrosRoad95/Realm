using RealmCore.ECS;

namespace RealmCore.Server.Interfaces;

public interface ISaveService
{
    event Action<Entity>? EntitySaved;

    Task<bool> Save(Entity entity);
    Task Commit();

    internal Task<int> SaveNewPlayerInventory(InventoryComponent inventoryComponent, int userId);
    internal Task<int> SaveNewVehicleInventory(InventoryComponent inventoryComponent, int vehicleId);
}
