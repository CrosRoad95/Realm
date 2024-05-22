namespace RealmCore.Persistence.Repository;

public interface IUserEventRepository
{
    Task AddEvent(int userId, int eventType, DateTime dateTime, string? metadata = null, CancellationToken cancellationToken = default);
    Task<UserEventData[]> GetAllEventsByUserId(int userId, IEnumerable<int>? events = null, CancellationToken cancellationToken = default);
    Task<UserEventData[]> GetLastEventsByUserId(int userId, int limit = 10, IEnumerable<int>? events = null, CancellationToken cancellationToken = default);
}

internal sealed class UserEventRepository : IUserEventRepository
{
    private readonly IDb _db;

    public UserEventRepository(IDb db)
    {
        _db = db;
    }

    public async Task AddEvent(int userId, int eventType, DateTime dateTime, string? metadata = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(AddEvent));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
            activity.AddTag("EventType", eventType);
            activity.AddTag("DateTime", dateTime);
            activity.AddTag("Metadata", metadata);
        }

        _db.UserEvents.Add(new UserEventData
        {
            UserId = userId,
            EventType = eventType,
            DateTime = dateTime,
            Metadata = metadata
        });
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserEventData[]> GetAllEventsByUserId(int userId, IEnumerable<int>? events = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetAllEventsByUserId));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
            activity.AddTag("Events", events);
        }

        var query = _db.UserEvents
            .AsNoTracking()
            .TagWithSource(nameof(VehicleEventRepository))
            .Where(x => x.UserId == userId);
        if (events != null)
            query = query.Where(x => events.Contains(x.EventType));
        return await query.ToArrayAsync(cancellationToken);
    }

    public async Task<UserEventData[]> GetLastEventsByUserId(int userId, int limit = 10, IEnumerable<int>? events = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetLastEventsByUserId));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
            activity.AddTag("Limit", limit);
            activity.AddTag("Events", events);
        }

        var query = _db.UserEvents
            .AsNoTracking()
            .TagWithSource(nameof(VehicleEventRepository))
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.DateTime)
            .Take(limit);
        if (events != null)
            query = query.Where(x => events.Contains(x.EventType));
        return await query.ToArrayAsync(cancellationToken);
    }

    public static readonly ActivitySource Activity = new("RealmCore.UserEventRepository", "1.0.0");
}
