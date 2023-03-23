using Realm.Persistance.DTOs;

namespace Realm.Server.Interfaces;

public interface IVehiclesService
{
    Task<Entity> ConvertToPrivateVehicle(Entity vehicleEntity);
    Task Despawn(Entity entity);
    Task<List<LightInfoVehicleDTO>> GetLightVehiclesByUserId(int userId);
    Task<List<Persistance.Data.Vehicle>> GetVehiclesByUserId(int userId);
    Task<Entity?> SpawnById(int id);
    internal Entity Spawn(Persistance.Data.Vehicle vehicleData);
}
