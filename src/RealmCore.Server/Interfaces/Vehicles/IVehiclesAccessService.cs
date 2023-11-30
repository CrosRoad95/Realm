using RealmCore.Server.Concepts.Access;

namespace RealmCore.Server.Interfaces.Vehicles;

public interface IVehiclesAccessService
{
    event Func<Ped, RealmVehicle, byte, bool>? CanEnter;
    event Action<Ped, RealmVehicle, byte, VehicleAccessController>? FailedToEnter;

    internal bool InternalCanEnter(Ped ped, RealmVehicle vehicle, byte seat, VehicleAccessController? vehicleAccessControllerComponent = null);
}
