namespace RealmCore.Server.Components.Vehicles.Access;

public class VehicleNoAccessComponent : VehicleAccessControllerComponent
{
    protected override bool CanEnter(Ped ped, Vehicle vehicle) => false;
}
