namespace RealmCore.Server.Modules.Vehicles.Access.Controllers;

public class VehicleNoAccessController : VehicleAccessController
{
    private VehicleNoAccessController() { }

    protected override bool CanEnter(Ped ped, RealmVehicle vehicle, byte seat) => false;

    public static VehicleNoAccessController Instance { get; } = new();
}
