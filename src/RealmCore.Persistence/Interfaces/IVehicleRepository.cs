namespace RealmCore.Persistence.Interfaces;

public interface IVehicleRepository
{
    Task<VehicleData> CreateVehicle(ushort model, DateTime now, CancellationToken cancellationToken = default);
    Task<List<VehicleData>> GetAllSpawnedVehicles(CancellationToken cancellationToken = default);
    Task<List<VehicleUserAccessData>> GetAllVehicleAccesses(int vehicleId, CancellationToken cancellationToken = default);
    Task<LightInfoVehicleDTO?> GetLightVehicleById(int vehicleId, CancellationToken cancellationToken = default);
    Task<List<LightInfoVehicleDTO>> GetLightVehiclesByUserId(int userId, CancellationToken cancellationToken = default);
    Task<List<int>> GetOwner(int vehicleId, CancellationToken cancellationToken = default);
    Task<VehicleData?> GetReadOnlyVehicleById(int id, CancellationToken cancellationToken = default);
    Task<VehicleData?> GetVehicleById(int id, CancellationToken cancellationToken = default);
    Task<List<VehicleData>> GetVehiclesByUserId(int userId, IEnumerable<int>? accessTypes = null, CancellationToken cancellationToken = default);
    Task<bool> HasUserAccessTo(int userId, int vehicleId, byte[]? accessType = null, CancellationToken cancellationToken = default);
    Task<bool> IsSpawned(int id, CancellationToken cancellationToken = default);
    Task<bool> SetKind(int id, byte kind, CancellationToken cancellationToken = default);
    Task<bool> SetSpawned(int id, bool spawned, CancellationToken cancellationToken = default);
    Task<bool> SoftRemove(int id, CancellationToken cancellationToken = default);
}
