namespace RealmCore.Server.Modules.Vehicles.Access;

internal sealed class VehicleAccessControllerLogic
{
    private readonly IVehiclesAccessService _vehicleAccessService;

    public VehicleAccessControllerLogic(IElementFactory elementFactory, IVehiclesAccessService vehicleAccessService)
    {
        _vehicleAccessService = vehicleAccessService;
        elementFactory.ElementCreated += HandleElementCreated;
    }

    private void HandleElementCreated(Element element)
    {
        if (element is not RealmVehicle vehicle)
            return;

        vehicle.CanEnter = HandleCanEnter;
        vehicle.CanExit = HandleCanExit;
    }

    private bool HandleCanEnter(Ped ped, Vehicle veh, byte seat)
    {
        var vehicle = (RealmVehicle)veh;
        if (!_vehicleAccessService.InternalCanEnter(ped, vehicle, seat, vehicle.AccessController))
            return false;

        return true;
    }

    private bool HandleCanExit(Ped ped, Vehicle veh, byte seat)
    {
        var vehicle = (RealmVehicle)veh;
        if (!_vehicleAccessService.InternalCanExit(ped, vehicle, seat, vehicle.AccessController))
            return false;

        if (ped is RealmPlayer realmPlayer)
        {
            realmPlayer.Admin.NoClip = false;
        }

        return true;
    }
}
