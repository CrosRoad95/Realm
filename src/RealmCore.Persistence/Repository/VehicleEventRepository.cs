namespace RealmCore.Persistence.Repository;

internal sealed class VehicleEventRepository : IVehicleEventRepository
{
    private readonly IDb _db;

    public VehicleEventRepository(IDb db)
    {
        _db = db;
    }

    public async Task AddEvent(int vehicleId, int eventType, DateTime dateTime, CancellationToken cancellationToken = default)
    {
        _db.VehicleEvents.Add(new VehicleEventData
        {
            VehicleId = vehicleId,
            EventType = eventType,
            DateTime = dateTime
        });
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<VehicleEventData>> GetAllEventsByVehicleId(int vehicleId, CancellationToken cancellationToken = default)
    {
        var query = _db.VehicleEvents
            .AsNoTracking()
            .TagWithSource(nameof(VehicleEventRepository))
            .Where(x => x.VehicleId == vehicleId);
        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }
}
