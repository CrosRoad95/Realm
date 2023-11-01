namespace RealmCore.Server.Logic.Components;

internal sealed class InventoryComponentLogic : ComponentLogic<InventoryComponent>
{
    private readonly ISaveService _saveService;

    public InventoryComponentLogic(IElementFactory elementFactory, ISaveService saveService) : base(elementFactory)
    {
        _saveService = saveService;
    }

    protected override async void ComponentAdded(InventoryComponent inventoryComponent)
    {
        if (inventoryComponent.Id != 0)
            return;

        if (inventoryComponent.Element is IComponents components)
        {
            if (components.TryGetComponent(out UserComponent userComponent))
            {
                var inventoryId = await _saveService.SaveNewPlayerInventory(inventoryComponent, userComponent.Id);
                inventoryComponent.Id = inventoryId;
            }
            else if (components.TryGetComponent(out PrivateVehicleComponent vehicle))
            {
                var inventoryId = await _saveService.SaveNewVehicleInventory(inventoryComponent, vehicle.Id);
                inventoryComponent.Id = inventoryId;
            }
        }
    }
}
