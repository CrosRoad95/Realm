namespace RealmCore.Server.Interfaces;

public interface IVehicleAccessService
{
    event Func<Entity, Entity, bool>? CanEnter;
    event Action<Entity, Entity, VehicleAccessControllerComponent>? FailedToEnter;

    internal bool InternalCanEnter(Ped ped, Vehicle vehicle, out Entity pedEntity, out Entity vehicleEntity);
    internal void RelayFailedToEnter(Entity pedEntity, Entity vehicleEntity, VehicleAccessControllerComponent vehicleAccessControllerComponent);
}
