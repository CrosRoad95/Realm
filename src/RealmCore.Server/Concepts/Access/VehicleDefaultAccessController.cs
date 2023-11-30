namespace RealmCore.Server.Concepts.Access;

public class VehicleDefaultAccessController : VehicleAccessController
{
    private VehicleDefaultAccessController() { }

    protected override bool CanEnter(Ped ped, RealmVehicle vehicle, byte seat) => true;

    public static VehicleDefaultAccessController Instance { get; } = new();
}
