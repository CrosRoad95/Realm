namespace RealmCore.Persistence.Repository;

public interface IUserEventRepository
{
    Task AddEvent(int userId, int eventType, DateTime dateTime, string? metadata = null, CancellationToken cancellationToken = default);
    Task<List<UserEventData>> GetAllEventsByUserId(int userId, IEnumerable<int>? events = null, CancellationToken cancellationToken = default);
    Task<List<UserEventData>> GetLastEventsByUserId(int userId, int limit = 10, IEnumerable<int>? events = null, CancellationToken cancellationToken = default);
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
        _db.UserEvents.Add(new UserEventData
        {
            UserId = userId,
            EventType = eventType,
            DateTime = dateTime,
            Metadata = metadata
        });
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<UserEventData>> GetAllEventsByUserId(int userId, IEnumerable<int>? events = null, CancellationToken cancellationToken = default)
    {
        var query = _db.UserEvents
            .AsNoTracking()
            .TagWithSource(nameof(VehicleEventRepository))
            .Where(x => x.UserId == userId);
        if (events != null)
            query = query.Where(x => events.Contains(x.EventType));
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<UserEventData>> GetLastEventsByUserId(int userId, int limit = 10, IEnumerable<int>? events = null, CancellationToken cancellationToken = default)
    {
        var query = _db.UserEvents
            .AsNoTracking()
            .TagWithSource(nameof(VehicleEventRepository))
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.DateTime)
            .Take(limit);
        if (events != null)
            query = query.Where(x => events.Contains(x.EventType));
        return await query.ToListAsync(cancellationToken);
    }
}
