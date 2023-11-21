namespace RealmCore.Server.Components.Vehicles.Access;

public class VehiclePrivateAccessControllerComponent : VehicleAccessControllerComponent
{
    protected override bool CanEnter(Ped ped, RealmVehicle vehicle, byte seat)
    {
        if (ped is not RealmPlayer player)
            return false;

        return vehicle.Access.HasAccess(player);
    }
}
