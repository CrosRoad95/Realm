namespace RealmCore.Server.Logic.Components;

internal sealed class InventoryComponentLogic : ComponentLogic<InventoryComponent>
{
    private readonly ISaveService _saveService;

    public InventoryComponentLogic(IEntityEngine entityEngine, ISaveService saveService) : base(entityEngine)
    {
        _saveService = saveService;
    }

    protected override async void ComponentAdded(InventoryComponent inventoryComponent)
    {
        if (inventoryComponent.Id != 0)
            return;

        var entity = inventoryComponent.Entity;
        if (entity.TryGetComponent(out UserComponent userComponent))
        {
            var inventoryId = await _saveService.SaveNewPlayerInventory(inventoryComponent, userComponent.Id);
            inventoryComponent.Id = inventoryId;
        }
        else if (entity.TryGetComponent(out PrivateVehicleComponent vehicle))
        {
            var inventoryId = await _saveService.SaveNewVehicleInventory(inventoryComponent, vehicle.Id);
            inventoryComponent.Id = inventoryId;
        }
    }
}
