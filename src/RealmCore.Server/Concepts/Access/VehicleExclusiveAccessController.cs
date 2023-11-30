namespace RealmCore.Server.Concepts.Access;

public class VehicleExclusiveAccessController : VehicleAccessController
{
    private readonly Ped _ped;

    public VehicleExclusiveAccessController(Ped ped)
    {
        _ped = ped;
    }

    protected override bool CanEnter(Ped ped, RealmVehicle vehicle, byte seat) => ped == _ped;
}
