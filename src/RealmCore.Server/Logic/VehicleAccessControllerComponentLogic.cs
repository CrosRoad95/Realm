namespace RealmCore.Server.Logic;

internal sealed class VehicleAccessControllerComponentLogic
{
    private readonly IVehiclesAccessService _vehicleAccessService;
    private readonly ILogger<VehicleAccessControllerComponentLogic> _logger;

    public VehicleAccessControllerComponentLogic(IElementFactory elementFactory, IVehiclesAccessService vehicleAccessService, ILogger<VehicleAccessControllerComponentLogic> logger)
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
