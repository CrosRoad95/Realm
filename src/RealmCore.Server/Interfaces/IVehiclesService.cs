using RealmCore.Persistance.DTOs;

namespace RealmCore.Server.Interfaces;

public interface IVehiclesService
{
    Task<Entity> ConvertToPrivateVehicle(Entity vehicleEntity);
    Task Despawn(Entity entity);
    Task<List<LightInfoVehicleDTO>> GetLightVehiclesByUserId(int userId);
    Task<List<VehicleData>> GetVehiclesByUserId(int userId);
    Task<Entity?> SpawnById(int id);
    internal Entity Spawn(VehicleData vehicleData);
}
