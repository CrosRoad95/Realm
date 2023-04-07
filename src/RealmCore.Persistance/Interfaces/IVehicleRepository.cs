namespace RealmCore.Persistance.Interfaces;

public interface IVehicleRepository : IRepositoryBase
{
    Task<VehicleData> CreateNewVehicle(ushort model, DateTime now);
    Task<List<VehicleData>> GetAllSpawnedVehicles();
    Task<List<LightInfoVehicleDTO>> GetLightVehiclesByUserId(int userId);
    Task<VehicleData?> GetReadOnlyVehicleById(int id);
    Task<List<VehicleData>> GetVehiclesByUserId(int userId);
    Task<bool> IsSpawned(int id);
    Task<bool> SetSpawned(int id, bool spawned);
}
