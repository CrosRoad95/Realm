namespace RealmCore.Server.Interfaces.Vehicles;

public interface IVehiclesAccessService
{
    event Func<Ped, RealmVehicle, byte, bool>? CanEnter;
    event Action<Ped, RealmVehicle, byte, VehicleAccessControllerComponent>? FailedToEnter;

    internal bool InternalCanEnter(Ped ped, RealmVehicle vehicle, byte seat, VehicleAccessControllerComponent? vehicleAccessControllerComponent = null);
}
