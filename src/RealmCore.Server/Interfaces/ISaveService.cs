namespace RealmCore.Server.Interfaces;

public interface ISaveService
{
    event Action<Element>? ElementSaved;

    Task<bool> BeginSave(Element element);
    Task Commit();
    Task<bool> Save(Element element);
    internal Task<int> SaveNewPlayerInventory(InventoryComponent inventoryComponent, int userId);
    internal Task<int> SaveNewVehicleInventory(InventoryComponent inventoryComponent, int vehicleId);
}
