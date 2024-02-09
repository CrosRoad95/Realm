namespace RealmCore.Server.Modules.Vehicles.Access.Controllers;

public class VehicleDefaultAccessController : VehicleAccessController
{
    private VehicleDefaultAccessController() { }

    protected override bool CanEnter(Ped ped, RealmVehicle vehicle, byte seat) => true;

    public static VehicleDefaultAccessController Instance { get; } = new();
}
