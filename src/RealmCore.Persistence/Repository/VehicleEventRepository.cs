namespace RealmCore.Persistence.Repository;

public interface IVehicleEventRepository
{
    Task AddEvent(int vehicleId, int eventType, DateTime dateTime, string? metadata = null, CancellationToken cancellationToken = default);
    Task<List<VehicleEventData>> GetAllEventsByVehicleId(int vehicleId, IEnumerable<int>? events = null, CancellationToken cancellationToken = default);
    Task<List<VehicleEventData>> GetLastEventsByVehicleId(int vehicleId, int limit = 10, IEnumerable<int>? events = null, CancellationToken cancellationToken = default);
}

internal sealed class VehicleEventRepository : IVehicleEventRepository
{
    private readonly IDb _db;

    public VehicleEventRepository(IDb db)
    {
        _db = db;
    }

    public async Task AddEvent(int vehicleId, int eventType, DateTime dateTime, string? metadata = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(AddEvent));

        if (activity != null)
        {
            activity.AddTag("VehicleId", vehicleId);
            activity.AddTag("EventType", eventType);
            activity.AddTag("DateTime", dateTime);
            activity.AddTag("Metadata", metadata);
        }

        _db.VehicleEvents.Add(new VehicleEventData
        {
            VehicleId = vehicleId,
            EventType = eventType,
            DateTime = dateTime,
            Metadata = metadata
        });
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<VehicleEventData>> GetAllEventsByVehicleId(int vehicleId, IEnumerable<int>? events = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetAllEventsByVehicleId));

        if (activity != null)
        {
            activity.AddTag("VehicleId", vehicleId);
            activity.AddTag("Events", events);
        }

        var query = _db.VehicleEvents
            .AsNoTracking()
            .TagWithSource(nameof(VehicleEventRepository))
            .Where(x => x.VehicleId == vehicleId);
        if (events != null)
            query = query.Where(x => events.Contains(x.EventType));
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<VehicleEventData>> GetLastEventsByVehicleId(int vehicleId, int limit = 10, IEnumerable<int>? events = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetLastEventsByVehicleId));

        if (activity != null)
        {
            activity.AddTag("VehicleId", vehicleId);
            activity.AddTag("Limit", limit);
            activity.AddTag("Events", events);
        }

        var query = _db.VehicleEvents
            .AsNoTracking()
            .TagWithSource(nameof(VehicleEventRepository))
            .Where(x => x.VehicleId == vehicleId)
            .OrderByDescending(x => x.DateTime)
            .Take(limit);
        if (events != null)
            query = query.Where(x => events.Contains(x.EventType));
        return await query.ToListAsync(cancellationToken);
    }

    public static readonly ActivitySource Activity = new("RealmCore.VehicleEventRepository", "1.0.0");
}
