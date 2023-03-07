namespace Realm.Persistance.Interfaces;

public interface IVehicleRepository : IRepositoryBase
{
    Task<Vehicle> CreateNewVehicle(ushort model);
    Task<List<Vehicle>> GetAllReadOnlySpawnedVehicles();
    Task<List<VehicleModelPositionDTO>> GetAllVehiclesModelPositionDTOsByUserId(int userId);
}
