namespace RealmCore.Server.Modules.Inventories;

internal sealed class InventoryLogic : PlayerLifecycle
{
    private readonly ILogger<InventoryLogic> _logger;

    public InventoryLogic(MtaServer server, ILogger<InventoryLogic> logger) : base(server)
    {
        _logger = logger;
    }

    private async Task InventoryCreated(Inventory inventory)
    {
        if (inventory.Id != 0)
            return;

        if (inventory.Owner is RealmPlayer player)
        {
            if (player.User.IsSignedIn)
            {
                var saveService = player.GetRequiredService<ISaveService>();
                var inventoryId = await saveService.SaveNewPlayerInventory(inventory, player.PersistentId);
                inventory.Id = inventoryId;
            }
        }
        else if (inventory.Owner is RealmVehicle vehicle)
        {
            if (vehicle.Persistence.IsLoaded)
            {
                var saveService = vehicle.GetRequiredService<ISaveService>();
                var inventoryId = await saveService.SaveNewVehicleInventory(inventory, vehicle.Persistence.Id);
                inventory.Id = inventoryId;
            }
        }
    }

    protected override void PlayerJoined(RealmPlayer player)
    {
        player.Inventory.PrimarySet += HandlePrimarySet;
    }

    protected override void PlayerLeft(RealmPlayer player)
    {
        player.Inventory.PrimarySet -= HandlePrimarySet;
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
