namespace RealmCore.Server.Modules.Inventories;

internal sealed class InventoryHostedService : PlayerLifecycle, IHostedService
{
    private readonly ILogger<InventoryHostedService> _logger;

    public InventoryHostedService(PlayersEventManager playersEventManager, ILogger<InventoryHostedService> logger) : base(playersEventManager)
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
        if (inventory.Id != 0)
            return;

        if (inventory.Owner is RealmPlayer player)
        {
            if (player.User.IsLoggedIn)
            {
                var saveService = player.GetRequiredService<IElementSaveService>();
                await saveService.SaveNewInventory(inventory);
            }
        }
        else if (inventory.Owner is RealmVehicle vehicle)
        {
            if (vehicle.Persistence.IsLoaded)
            {
                var saveService = vehicle.GetRequiredService<IElementSaveService>();
                await saveService.SaveNewInventory(inventory);
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
