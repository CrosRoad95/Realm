namespace Realm.Persistance.Interfaces;

public interface IVehicleRepository : IRepositoryBase
{
    Task<Vehicle> CreateNewVehicle(ushort model);
    Task<List<Vehicle>> GetAllSpawnedVehicles();
    Task<List<LightInfoVehicleDTO>> GetLightVehiclesByUserId(int userId);
    Task<List<Vehicle>> GetVehiclesByUserId(int userId);
}
