namespace RealmCore.Server.Logic.Components;

internal class VehicleAccessControllerComponentLogic : ComponentLogic<VehicleAccessControllerComponent>
{
    private readonly IVehicleAccessService _vehicleAccessService;

    public VehicleAccessControllerComponentLogic(IECS ecs, IVehicleAccessService vehicleAccessService) : base(ecs)
    {
        _vehicleAccessService = vehicleAccessService;
    }

    protected override void ComponentAdded(VehicleAccessControllerComponent vehicleAccessControllerComponent)
    {
        vehicleAccessControllerComponent.Entity.GetRequiredComponent<VehicleElementComponent>().Vehicle.CanEnter = (ped, vehicle) =>
        {
            if (_vehicleAccessService.InternalCanEnter(ped, vehicle, out var pedEntity, out var vehicleEntity))
                return true;

            if (!vehicleAccessControllerComponent.InternalCanEnter(pedEntity, vehicleEntity))
            {
                _vehicleAccessService.RelayFailedToEnter(pedEntity, vehicleEntity, vehicleAccessControllerComponent);
                return false;
            }

            return true;
        };
        base.ComponentAdded(vehicleAccessControllerComponent);
    }

    protected override void ComponentRemoved(VehicleAccessControllerComponent vehicleAccessControllerComponent)
    {
        vehicleAccessControllerComponent.Entity.GetRequiredComponent<VehicleElementComponent>().Vehicle.CanEnter = null;
    }
}
