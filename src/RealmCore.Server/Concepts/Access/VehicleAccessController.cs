namespace RealmCore.Server.Concepts.Access;

[ComponentUsage(true)]
public abstract class VehicleAccessController
{
    protected abstract bool CanEnter(Ped ped, RealmVehicle vehicle, byte seat);
    internal bool InternalCanEnter(Ped ped, RealmVehicle vehicle, byte seat) => CanEnter(ped, vehicle, seat);
}
