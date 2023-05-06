namespace RealmCore.Persistance.Repository;

internal class VehicleEventRepository : IVehicleEventRepository
{
    private readonly IDb _db;

    public VehicleEventRepository(IDb db)
    {
        _db = db;
    }

    public void AddEvent(int vehicleId, int eventType, DateTime dateTime)
    {
        _db.VehicleEvents.Add(new VehicleEventData
        {
            VehicleId = vehicleId,
            EventType = eventType,
            DateTime = dateTime
        });
    }

    public async Task<List<VehicleEventDTO>> GetAllEventsByVehicleId(int vehicleId)
    {
        return await _db.VehicleEvents
            .Where(x => x.VehicleId == vehicleId)
            .Select(x => new VehicleEventDTO
            {
                EventType = x.EventType,
                DateTime = x.DateTime
            })
            .ToListAsync();
    }

    public Task<int> Commit()
    {
        return _db.SaveChangesAsync();
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
