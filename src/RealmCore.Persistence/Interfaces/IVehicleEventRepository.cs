namespace RealmCore.Persistence.Interfaces;

public interface IVehicleEventRepository
{
    Task AddEvent(int vehicleId, int eventType, DateTime dateTime, CancellationToken cancellationToken = default);
    Task<List<VehicleEventData>> GetAllEventsByVehicleId(int vehicleId, CancellationToken cancellationToken = default);
}
