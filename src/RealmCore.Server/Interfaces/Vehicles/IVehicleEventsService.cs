namespace RealmCore.Server.Interfaces.Vehicles;

public interface IVehicleEventsService : IVehicleService, IEnumerable<VehicleEventDTO>
{
    void AddEvent(int eventType, string? metadata = null);
}
