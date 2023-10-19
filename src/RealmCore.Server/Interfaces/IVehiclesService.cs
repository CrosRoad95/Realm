using RealmCore.Persistence.DTOs;

namespace RealmCore.Server.Interfaces;

public interface IVehiclesService
{
    Task<bool> AddVehicleEvent(Entity vehicleEntity, int eventId, string? metadata = null);
    Task<Entity> ConvertToPrivateVehicle(Entity vehicleEntity);
    Task<Entity> CreateVehicle(ushort model, Vector3 position, Vector3 rotation);
    Task Destroy(Entity entity);
    Task<List<VehicleData>> GetAllSpawnedVehicles();
    Task<List<VehicleEventData>> GetAllVehicleEvents(Entity vehicleEntity, IEnumerable<int>? events = null);
    Task<List<LightInfoVehicleDTO>> GetAllLightVehicles(Entity entity);
    Task<VehicleAccess?> GetVehicleAccess(Entity vehicleEntity);
    Task<List<VehicleData>> GetAllVehicles(Entity entity);
    Task<bool> SetVehicleKind(Entity vehicleEntity, byte kind);
    Task<bool> SetVehicleKind(int id, byte kind);
    Task<bool> SetVehicleSpawned(Entity vehicleEntity, bool spawned = true);
    Task<Entity> Spawn(VehicleData vehicleData);
    Task<List<VehicleEventData>> GetLastVehicleEvents(Entity vehicleEntity, int limit = 10, IEnumerable<int>? events = null);
    IEnumerable<Entity> GetOnlineOwner(Entity vehicleEntity);
}
