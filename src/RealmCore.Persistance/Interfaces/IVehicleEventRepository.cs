namespace RealmCore.Persistance.Interfaces;

public interface IVehicleEventRepository : IRepositoryBase
{
    void AddEvent(int vehicleId, int eventType, DateTime dateTime);
    Task<int> Commit();
    Task<List<VehicleEventDTO>> GetAllEventsByVehicleId(int vehicleId);
}
