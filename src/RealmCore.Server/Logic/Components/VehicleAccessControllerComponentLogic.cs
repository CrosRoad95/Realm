namespace RealmCore.Server.Logic.Components;

internal sealed class VehicleAccessControllerComponentLogic : ComponentLogic<VehicleAccessControllerComponent>
{
    private readonly IVehicleAccessService _vehicleAccessService;
    private readonly ILogger<VehicleAccessControllerComponentLogic> _logger;

    public VehicleAccessControllerComponentLogic(IElementFactory elementFactory, IVehicleAccessService vehicleAccessService, ILogger<VehicleAccessControllerComponentLogic> logger) : base(elementFactory)
    {
        _vehicleAccessService = vehicleAccessService;
        _logger = logger;
    }

    protected override void ComponentAdded(VehicleAccessControllerComponent vehicleAccessControllerComponent)
    {
        var vehicle = (RealmVehicle)vehicleAccessControllerComponent.Element;
        vehicle.CanEnter = (ped, vehicle, seat) =>
        {
            if (!_vehicleAccessService.InternalCanEnter(ped, (RealmVehicle)vehicle, seat, vehicleAccessControllerComponent))
                return false;

            return true;
        };
    }

    protected override void ComponentDetached(VehicleAccessControllerComponent vehicleAccessControllerComponent)
    {
        var vehicle = (RealmVehicle)vehicleAccessControllerComponent.Element;
        vehicle.CanEnter = null;
    }
}
