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
        var realmVehicle = (RealmVehicle)vehicleAccessControllerComponent.Element;
        realmVehicle.CanEnter = (ped, vehicle) =>
        {
            // TODO: add seat
            if (!_vehicleAccessService.InternalCanEnter(ped, (RealmVehicle)vehicle, 0, vehicleAccessControllerComponent))
                return false;

            return true;
        };
    }

    protected override void ComponentDetached(VehicleAccessControllerComponent vehicleAccessControllerComponent)
    {
        var realmVehicle = (RealmVehicle)vehicleAccessControllerComponent.Element;
        realmVehicle.CanEnter = null;
    }
}
