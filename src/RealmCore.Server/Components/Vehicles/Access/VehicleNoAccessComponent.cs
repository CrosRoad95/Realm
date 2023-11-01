
namespace RealmCore.Server.Components.Vehicles.Access;

public class VehicleNoAccessComponent : VehicleAccessControllerComponent
{
    protected override bool CanEnter(Ped ped, RealmVehicle vehicle, byte seat) => false;
}
