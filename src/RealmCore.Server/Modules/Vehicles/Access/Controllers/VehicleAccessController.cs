namespace RealmCore.Server.Modules.Vehicles.Access.Controllers;

public abstract class VehicleAccessController
{
    protected abstract bool CanEnter(Ped ped, RealmVehicle vehicle, byte seat);
    internal bool InternalCanEnter(Ped ped, RealmVehicle vehicle, byte seat) => CanEnter(ped, vehicle, seat);
}
