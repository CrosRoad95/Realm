using RealmCore.Persistance.DTOs;

namespace RealmCore.Server.Interfaces;

public interface IVehiclesService
{
    Task AddVehicleEvent(int id, int eventId);
    Task AddVehicleEvent(Entity entity, int eventId);
    Task<Entity> ConvertToPrivateVehicle(Entity vehicleEntity);
    Task Despawn(Entity entity);
    Task<List<VehicleEventDTO>> GetAllVehicleEvents(int id);
    Task<List<VehicleEventDTO>> GetAllVehicleEvents(Entity entity);
    Task<List<LightInfoVehicleDTO>> GetLightVehiclesByUserId(int userId);
    Task<List<VehicleData>> GetVehiclesByUserId(int userId);
    Task<Entity?> SpawnById(int id);
    internal Entity Spawn(VehicleData vehicleData);
}
