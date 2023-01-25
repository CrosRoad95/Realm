using Realm.Domain.Data;

namespace Realm.Server.Interfaces;

public interface IVehiclesService
{
    Task<Entity> ConvertToPrivateVehicle(Entity vehicleEntity);
    Task<List<VehicleLightInfo>> GetAllVehiclesLightInfoByOwnerId(Guid userId);
}
