namespace RealmCore.Persistence.Repository;

internal sealed class VehicleEventRepository : IVehicleEventRepository
{
    private readonly IDb _db;

    public VehicleEventRepository(IDb db)
    {
        _db = db;
    }

    public async Task AddEvent(int vehicleId, int eventType, DateTime dateTime, string? metadata = null, CancellationToken cancellationToken = default)
    {
        _db.VehicleEvents.Add(new VehicleEventData
        {
            VehicleId = vehicleId,
            EventType = eventType,
            DateTime = dateTime,
            Metadata = metadata
        });
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<VehicleEventData>> GetAllEventsByVehicleId(int vehicleId, IEnumerable<int>? events = null, CancellationToken cancellationToken = default)
    {
        var query = _db.VehicleEvents
            .AsNoTracking()
            .TagWithSource(nameof(VehicleEventRepository))
            .Where(x => x.VehicleId == vehicleId);
        if (events != null)
            query = query.Where(x => events.Contains(x.EventType));
        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<VehicleEventData>> GetLastEventsByVehicleId(int vehicleId, int limit = 10, IEnumerable<int>? events = null, CancellationToken cancellationToken = default)
    {
        var query = _db.VehicleEvents
            .AsNoTracking()
            .TagWithSource(nameof(VehicleEventRepository))
            .Where(x => x.VehicleId == vehicleId)
            .OrderByDescending(x => x.DateTime)
            .Take(limit);
        if (events != null)
            query = query.Where(x => events.Contains(x.EventType));
        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }
}
