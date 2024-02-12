namespace RealmCore.Server.Modules.Inventories;

internal sealed class InventoryLogic : PlayerLogic
{
    private readonly ISaveService _saveService;
    private readonly ILogger<InventoryLogic> _logger;

    public InventoryLogic(MtaServer mtaServer, ISaveService saveService, ILogger<InventoryLogic> logger) : base(mtaServer)
    {
        _saveService = saveService;
        _logger = logger;
    }

    private async Task InventoryCreated(Inventory inventory)
    {
        if (inventory.Id != 0)
            return;

        if (inventory.Owner is RealmPlayer player)
        {
            var inventoryId = await _saveService.SaveNewPlayerInventory(inventory, player.PersistentId);
            inventory.Id = inventoryId;
        }
        else if (inventory.Owner is RealmVehicle vehicle)
        {
            if (vehicle.Persistence.IsLoaded)
            {
                var inventoryId = await _saveService.SaveNewVehicleInventory(inventory, vehicle.Persistence.Id);
                inventory.Id = inventoryId;
            }
        }
    }

    protected override void PlayerJoined(RealmPlayer player)
    {
        player.Inventory.PrimarySet += HandlePrimarySet;
    }

    private async void HandlePrimarySet(IElementInventoryFeature inventoryService, Inventory inventory)
    {
        try
        {
            await InventoryCreated(inventory);
        }
        catch (Exception ex)
        {
            _logger.LogHandleError(ex);
        }
    }
}
