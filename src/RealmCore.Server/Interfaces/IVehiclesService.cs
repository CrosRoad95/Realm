namespace RealmCore.Server.Interfaces;

public interface IVehiclesService
{
    Task<RealmVehicle> ConvertToPersistantVehicle(RealmVehicle vehicle);
    Task<RealmVehicle> CreatePersistantVehicle(ushort model, Vector3 position, Vector3 rotation);
    Task Destroy(RealmVehicle vehicle);
    Task<List<VehicleData>> GetAllSpawnedVehicles();
    Task<List<LightInfoVehicleDTO>> GetAllLightVehicles(RealmPlayer player);
    Task<List<VehicleData>> GetAllVehicles(RealmPlayer player);
    Task<bool> SetVehicleSpawned(RealmVehicle vehicle, bool spawned = true);
    Task<RealmVehicle> Spawn(VehicleData vehicleData);
    IEnumerable<RealmPlayer> GetOnlineOwner(RealmVehicle vehicle);
}
