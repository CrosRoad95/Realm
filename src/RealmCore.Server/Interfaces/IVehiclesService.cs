namespace RealmCore.Server.Interfaces;

public interface IVehiclesService
{
    Task<bool> AddVehicleEvent(RealmVehicle vehicle, int eventId, string? metadata = null);
    Task<RealmVehicle> ConvertToPrivateVehicle(RealmVehicle vehicle);
    Task<RealmVehicle> CreateVehicle(ushort model, Vector3 position, Vector3 rotation);
    Task Destroy(RealmVehicle vehicle);
    Task<List<VehicleData>> GetAllSpawnedVehicles();
    Task<List<VehicleEventData>> GetAllVehicleEvents(RealmVehicle vehicle, IEnumerable<int>? events = null);
    Task<List<LightInfoVehicleDTO>> GetAllLightVehicles(RealmPlayer player);
    Task<VehicleAccess?> GetVehicleAccess(RealmVehicle vehicle);
    Task<List<VehicleData>> GetAllVehicles(RealmPlayer player);
    Task<bool> SetVehicleSpawned(RealmVehicle vehicle, bool spawned = true);
    Task<RealmVehicle> Spawn(VehicleData vehicleData);
    Task<List<VehicleEventData>> GetLastVehicleEvents(RealmVehicle vehicle, int limit = 10, IEnumerable<int>? events = null);
    IEnumerable<RealmPlayer> GetOnlineOwner(RealmVehicle vehicle);
}
