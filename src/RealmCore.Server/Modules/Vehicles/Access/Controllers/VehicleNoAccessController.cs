namespace RealmCore.Server.Modules.Vehicles.Access.Controllers;

public sealed class VehicleNoAccessController : VehicleAccessController
{
    private VehicleNoAccessController() { }

    protected override bool CanEnter(Ped ped, RealmVehicle vehicle, byte seat) => false;
    protected override bool CanExit(Ped ped, RealmVehicle vehicle, byte seat) => false;

    public static VehicleNoAccessController Instance { get; } = new();
}
