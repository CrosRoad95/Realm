using RealmCore.Persistence.DTOs;

namespace RealmCore.Persistence.Interfaces;

public interface IVehicleRepository : IRepositoryBase
{
    Task<VehicleData> CreateNewVehicle(ushort model, DateTime now);
    Task<List<VehicleData>> GetAllSpawnedVehicles();
    Task<LightInfoVehicleDTO?> GetLightVehicleById(int vehicleId);
    Task<List<LightInfoVehicleDTO>> GetLightVehiclesByUserId(int userId);
    Task<VehicleData?> GetReadOnlyVehicleById(int id);
    Task<VehicleData?> GetVehicleById(int id);
    Task<List<VehicleData>> GetVehiclesByUserId(int userId);
    Task<bool> IsSpawned(int id);
    Task<bool> SetKind(int id, byte kind);
    Task<bool> SetSpawned(int id, bool spawned);
    Task<bool> SoftRemove(int id);
}
