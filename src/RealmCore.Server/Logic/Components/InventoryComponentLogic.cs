namespace RealmCore.Server.Logic.Components;

internal sealed class InventoryComponentLogic : ComponentLogic<InventoryComponent>
{
    private readonly ISaveService _saveService;
    private readonly ILogger<InventoryComponentLogic> _logger;

    public InventoryComponentLogic(IElementFactory elementFactory, ISaveService saveService, ILogger<InventoryComponentLogic> logger) : base(elementFactory)
    {
        _saveService = saveService;
        _logger = logger;
    }

    private async Task ComponentAddedCore(InventoryComponent inventoryComponent)
    {
        if (inventoryComponent.Id != 0)
            return;

        if (inventoryComponent.Element is RealmPlayer player)
        {
            var inventoryId = await _saveService.SaveNewPlayerInventory(inventoryComponent, player.UserId);
            inventoryComponent.Id = inventoryId;
        }
        else if (inventoryComponent.Element is RealmVehicle vehicle)
        {
            if(vehicle.Persistance.IsLoaded)
            {
                var inventoryId = await _saveService.SaveNewVehicleInventory(inventoryComponent, vehicle.Persistance.Id);
                inventoryComponent.Id = inventoryId;
            }
        }
    }

    protected override async void ComponentAdded(InventoryComponent inventoryComponent)
    {
        try
        {
            await ComponentAddedCore(inventoryComponent);
        }
        catch(Exception ex)
        {
            _logger.LogHandleError(ex);
        }
    }
}
