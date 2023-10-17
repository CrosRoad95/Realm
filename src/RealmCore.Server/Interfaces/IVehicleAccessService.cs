namespace RealmCore.Server.Interfaces;

public interface IVehicleAccessService
{
    event Func<Entity, Entity, bool>? CanEnter;
    event Action<Entity, Entity, VehicleAccessControllerComponent>? FailedToEnter;

    internal bool InternalCanEnter(Entity pedEntity, Entity vehicleEntity, VehicleAccessControllerComponent? vehicleAccessControllerComponent = null);
}
