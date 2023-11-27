namespace RealmCore.Server.Interfaces;

public interface IVehiclesService
{
    Task<RealmVehicle> ConvertToPersistantVehicle(RealmVehicle vehicle, CancellationToken cancellationToken = default);
    Task<RealmVehicle> CreatePersistantVehicle(ushort model, Vector3 position, Vector3 rotation, CancellationToken cancellationToken = default);
    Task<List<VehicleData>> GetAllSpawnedVehicles(CancellationToken cancellationToken = default);
    Task<List<LightInfoVehicleDTO>> GetAllLightVehicles(RealmPlayer player, CancellationToken cancellationToken = default);
    Task<List<VehicleData>> GetAllVehicles(RealmPlayer player, CancellationToken cancellationToken = default);
    Task<bool> SetVehicleSpawned(RealmVehicle vehicle, bool spawned = true, CancellationToken cancellationToken = default);
    Task<RealmVehicle> Spawn(VehicleData vehicleData, CancellationToken cancellationToken = default);
    IEnumerable<RealmPlayer> GetOnlineOwners(RealmVehicle vehicle);
}
