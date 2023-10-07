namespace RealmCore.Persistence.Interfaces;

public interface IVehicleEventRepository
{
    Task AddEvent(int vehicleId, int eventType, DateTime dateTime, string? metadata = null, CancellationToken cancellationToken = default);
    Task<List<VehicleEventData>> GetAllEventsByVehicleId(int vehicleId, IEnumerable<int>? events = null, CancellationToken cancellationToken = default);
    Task<List<VehicleEventData>> GetLastEventsByVehicleId(int vehicleId, int limit = 10, IEnumerable<int>? events = null, CancellationToken cancellationToken = default);
}
