namespace RealmCore.Server.Modules.Vehicles.Access.Controllers;

public abstract class VehicleAccessController
{
    protected abstract bool CanEnter(Ped ped, RealmVehicle vehicle, byte seat);
    protected virtual bool CanExit(Ped ped, RealmVehicle vehicle, byte seat) { return true; }
    internal bool InternalCanEnter(Ped ped, RealmVehicle vehicle, byte seat) => CanEnter(ped, vehicle, seat);
    internal bool InternalCanExit(Ped ped, RealmVehicle vehicle, byte seat) => CanExit(ped, vehicle, seat);
}
