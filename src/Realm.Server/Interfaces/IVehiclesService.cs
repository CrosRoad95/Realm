namespace Realm.Server.Interfaces;

public interface IVehiclesService
{
    Task<Entity> ConvertToPrivateVehicle(Entity vehicleEntity);
}
