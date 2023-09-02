namespace RealmCore.Persistence.Interfaces;

public interface IVehicleEventRepository
{
    Task AddEvent(int vehicleId, int eventType, DateTime dateTime);
    Task<List<VehicleEventData>> GetAllEventsByVehicleId(int vehicleId);
}
