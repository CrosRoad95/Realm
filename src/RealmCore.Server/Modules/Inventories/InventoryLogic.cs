namespace RealmCore.Server.Modules.Inventories;

internal sealed class InventoryLogic : PlayerLifecycle, IHostedService
{
    private readonly ILogger<InventoryLogic> _logger;

    public InventoryLogic(PlayersEventManager playersEventManager, ILogger<InventoryLogic> logger) : base(playersEventManager)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task InventoryCreated(Inventory inventory)
    {
        if (inventory is not PersistentElementInventory persistentElementInventory)
            return;

        if (persistentElementInventory.Id != 0)
            return;

        if (persistentElementInventory.Owner is RealmPlayer player)
        {
            if (player.User.IsLoggedIn)
            {
                var saveService = player.GetRequiredService<ElementSaveService>();
                await saveService.SaveNewInventory(persistentElementInventory);
            }
        }
        else if (persistentElementInventory.Owner is RealmVehicle vehicle)
        {
            if (vehicle.Persistence.IsLoaded)
            {
                var saveService = vehicle.GetRequiredService<ElementSaveService>();
                await saveService.SaveNewInventory(persistentElementInventory);
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
