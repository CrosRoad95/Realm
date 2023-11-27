namespace RealmCore.Server.Interfaces;

public interface ISaveService
{
    event Action<Element>? ElementSaved;

    Task<bool> Save(Element element, CancellationToken cancellationToken = default);
    internal Task<int> SaveNewPlayerInventory(InventoryComponent inventoryComponent, int userId, CancellationToken cancellationToken = default);
    internal Task<int> SaveNewVehicleInventory(InventoryComponent inventoryComponent, int vehicleId, CancellationToken cancellationToken = default);
}
