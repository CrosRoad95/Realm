namespace RealmCore.Server.Modules.Vehicles.Access.Controllers;

public class VehiclePrivateAccessController : VehicleAccessController
{
    protected override bool CanEnter(Ped ped, RealmVehicle vehicle, byte seat)
    {
        if (ped is not RealmPlayer player)
            return false;

        return vehicle.Access.HasAccess(player);
    }
}
