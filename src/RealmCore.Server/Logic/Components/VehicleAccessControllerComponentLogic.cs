namespace RealmCore.Server.Logic.Components;

internal sealed class VehicleAccessControllerComponentLogic : ComponentLogic<VehicleAccessControllerComponent>
{
    private readonly IEntityEngine _entityEngine;
    private readonly IVehicleAccessService _vehicleAccessService;
    private readonly ILogger<VehicleAccessControllerComponentLogic> _logger;

    public VehicleAccessControllerComponentLogic(IEntityEngine entityEngine, IVehicleAccessService vehicleAccessService, ILogger<VehicleAccessControllerComponentLogic> logger) : base(entityEngine)
    {
        _entityEngine = entityEngine;
        _vehicleAccessService = vehicleAccessService;
        _logger = logger;
    }

    protected override void ComponentAdded(VehicleAccessControllerComponent vehicleAccessControllerComponent)
    {
        vehicleAccessControllerComponent.Entity.GetRequiredComponent<VehicleElementComponent>().Vehicle.CanEnter = (ped, vehicle) =>
        {
            if (!_entityEngine.TryGetEntityByPed(ped, out var pedEntity) || !_entityEngine.TryGetByElement(vehicle, out var vehicleEntity) || pedEntity == null || vehicleEntity == null)
            {
                using var _ = _logger.BeginElement(ped);
                _logger.LogWarning("Player/ped attempted to enter enter vehicle that has no entity.");
                return false;
            }

            if (!_vehicleAccessService.InternalCanEnter(pedEntity, vehicleEntity, vehicleAccessControllerComponent))
                return false;

            return true;
        };
    }

    protected override void ComponentDetached(VehicleAccessControllerComponent vehicleAccessControllerComponent)
    {
        vehicleAccessControllerComponent.Entity.GetRequiredComponent<VehicleElementComponent>().Vehicle.CanEnter = null;
    }
}
