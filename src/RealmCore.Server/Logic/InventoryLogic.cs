using RealmCore.Server.Services.Elements;

namespace RealmCore.Server.Logic;

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
            var inventoryId = await _saveService.SaveNewPlayerInventory(inventory, player.UserId);
            inventory.Id = inventoryId;
        }
        else if (inventory.Owner is RealmVehicle vehicle)
        {
            if (vehicle.Persistance.IsLoaded)
            {
                var inventoryId = await _saveService.SaveNewVehicleInventory(inventory, vehicle.Persistance.Id);
                inventory.Id = inventoryId;
            }
        }
    }

    protected override void PlayerJoined(RealmPlayer player)
    {
        player.Inventory.PrimarySet += HandlePrimarySet;
    }

    private async void HandlePrimarySet(IElementInventoryService inventoryService, Inventory inventory)
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

    protected override void PlayerLeft(RealmPlayer player)
    {

    }
}
