namespace RealmCore.Server.Concepts.Access;

public class VehicleNoAccessController : VehicleAccessController
{
    private VehicleNoAccessController() { }

    protected override bool CanEnter(Ped ped, RealmVehicle vehicle, byte seat) => false;

    public static VehicleNoAccessController Instance { get; } = new();
}
