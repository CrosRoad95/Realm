using RealmCore.Persistence.DTOs;

namespace RealmCore.Server.Interfaces;

public interface IVehiclesService
{
    Task AddVehicleEvent(int id, int eventId);
    Task AddVehicleEvent(Entity entity, int eventId);
    Task<Entity> ConvertToPrivateVehicle(Entity vehicleEntity);
    Task Despawn(Entity entity);
    Task<List<VehicleEventDTO>> GetAllVehicleEvents(int id);
    Task<List<VehicleEventDTO>> GetAllVehicleEvents(Entity entity);
    Task<LightInfoVehicleDTO?> GetLightVehicleById(int vehicleId);
    Task<List<LightInfoVehicleDTO>> GetLightVehiclesByUserId(int userId);
    Task<List<VehicleData>> GetVehiclesByUserId(int userId);
    Task<bool> SetVehicleKind(int id, byte kind);
    Task<bool> SetVehicleKind(Entity vehicleEntity, byte kind);
    Task<bool> SetVehicleSpawned(Entity vehicleEntity, bool spawned = true);
    Task<bool> SetVehicleSpawned(int id, bool spawned = true);
    internal Entity Spawn(VehicleData vehicleData);
}
