
namespace RealmCore.Server.Components.Vehicles.Access;

public class VehicleDefaultAccessComponent : VehicleAccessControllerComponent
{
    protected override bool CanEnter(Ped ped, RealmVehicle vehicle, byte seat) => true;
}
