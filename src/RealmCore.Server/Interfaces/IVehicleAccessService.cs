namespace RealmCore.Server.Interfaces;

public interface IVehicleAccessService
{
    event Func<Entity, Entity, bool>? CanEnter;

    internal bool InternalCanEnter(Ped ped, Vehicle vehicle, out Entity pedEntity, out Entity vehicleEntity);
}
