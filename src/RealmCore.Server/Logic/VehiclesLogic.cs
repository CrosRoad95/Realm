﻿namespace RealmCore.Server.Logic;

internal class VehiclesLogic
{
    private readonly IECS _ecs;
    private readonly ISaveService _saveService;
    private readonly ILogger<PlayersLogic> _logger;

    public VehiclesLogic(IECS ecs, ISaveService saveService, ILogger<PlayersLogic> logger)
    {
        _ecs = ecs;
        _saveService = saveService;
        _logger = logger;
        _ecs.EntityCreated += HandleEntityCreated;
    }

    private void HandleEntityCreated(Entity entity)
    {
        if (entity.Tag != EntityTag.Vehicle)
            return;

        entity.Disposed += HandleEntityDestroyed;
        entity.ComponentAdded += HandleComponentAdded;
    }

    private void HandleEntityDestroyed(Entity entity)
    {
        entity.ComponentAdded -= HandleComponentAdded;
    }

    private async void HandleComponentAdded(Component component)
    {
        try
        {
            var entity = component.Entity;

            if (component is InventoryComponent inventoryComponent)
            {
                if (inventoryComponent.Id == 0)
                {
                    if (entity.TryGetComponent(out PrivateVehicleComponent vehicle))
                    {
                        var inventoryId = await _saveService.SaveNewPlayerInventory(inventoryComponent, vehicle.Id);
                        inventoryComponent.Id = inventoryId;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Something went wrong");
        }
    }
}