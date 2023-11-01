namespace RealmCore.Server.Components.Vehicles.Access;

[ComponentUsage(true)]
public abstract class VehicleAccessControllerComponent : Component
{
    protected abstract bool CanEnter(Ped ped, RealmVehicle vehicle, byte seat);
    internal bool InternalCanEnter(Ped ped, RealmVehicle vehicle, byte seat) => CanEnter(ped, vehicle, seat);
}
