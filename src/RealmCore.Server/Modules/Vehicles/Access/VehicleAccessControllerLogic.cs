namespace RealmCore.Server.Modules.Vehicles.Access;

internal sealed class VehicleAccessControllerLogic
{
    private readonly IVehiclesAccessService _vehicleAccessService;
    private readonly ILogger<VehicleAccessControllerLogic> _logger;

    public VehicleAccessControllerLogic(IElementFactory elementFactory, IVehiclesAccessService vehicleAccessService, ILogger<VehicleAccessControllerLogic> logger)
    {
        _vehicleAccessService = vehicleAccessService;
        _logger = logger;
        elementFactory.ElementCreated += HandleElementCreated;
    }

    private void HandleElementCreated(Element element)
    {
        if (element is not RealmVehicle vehicle)
            return;

        vehicle.CanEnter = HandleCanEnter;
    }

    private bool HandleCanEnter(Ped ped, Vehicle veh, byte seat)
    {
        var vehicle = (RealmVehicle)veh;
        if (!_vehicleAccessService.InternalCanEnter(ped, vehicle, seat, vehicle.AccessController))
            return false;

        return true;
    }
}
