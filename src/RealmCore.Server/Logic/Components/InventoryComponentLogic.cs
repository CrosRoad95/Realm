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
        if (inventoryComponent.Id == 0)
        {
            if (inventoryComponent.Entity.TryGetComponent(out UserComponent userComponent))
            {
                var inventoryId = await _saveService.SaveNewPlayerInventory(inventoryComponent, userComponent.Id);
                inventoryComponent.Id = inventoryId;
            }
        }
    }
}
