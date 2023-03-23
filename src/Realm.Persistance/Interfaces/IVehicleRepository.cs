namespace Realm.Persistance.Interfaces;

public interface IVehicleRepository : IRepositoryBase
{
    Task<Vehicle> CreateNewVehicle(ushort model);
    Task<List<Vehicle>> GetAllSpawnedVehicles();
    Task<List<LightInfoVehicleDTO>> GetLightVehiclesByUserId(int userId);
    Task<Vehicle?> GetReadOnlyVehicleById(int id);
    Task<List<Vehicle>> GetVehiclesByUserId(int userId);
    Task<bool> IsSpawned(int id);
    Task<bool> SetSpawned(int id, bool spawned);
}
