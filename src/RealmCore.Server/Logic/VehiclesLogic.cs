using RealmCore.ECS;
using RealmCore.ECS.Components;

namespace RealmCore.Server.Logic;

internal class VehiclesLogic
{
    private readonly IEntityEngine _ecs;
    private readonly ISaveService _saveService;
    private readonly ILogger<PlayersLogic> _logger;
    private readonly IVehicleAccessService _vehicleAccessService;

    public VehiclesLogic(IEntityEngine ecs, ISaveService saveService, ILogger<PlayersLogic> logger, IVehicleAccessService vehicleAccessService)
    {
        _ecs = ecs;
        _saveService = saveService;
        _logger = logger;
        _vehicleAccessService = vehicleAccessService;
        _ecs.EntityCreated += HandleEntityCreated;

        _vehicleAccessService.CanEnter += HandleCanEnter;
    }

    private bool HandleCanEnter(Entity pedEntity, Entity vehicleEntity)
    {

        return false;
    }

    private void HandleEntityCreated(Entity entity)
    {
        if (!entity.HasComponent<VehicleTagComponent>())
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
