namespace RealmCore.Persistence.Repository;

public sealed class UserLoginHistoryRepository
{
    private readonly IDb _db;

    public UserLoginHistoryRepository(IDb db)
    {
        _db = db;
    }

    public async Task Add(int userId, DateTime now, string ip, string serial, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(Add));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
            activity.AddTag("now", now);
            activity.AddTag("Ip", ip);
            activity.AddTag("Serial", serial);
        }

        var userLoginHistoryData = new UserLoginHistoryData
        {
            UserId = userId,
            DateTime = now,
            Ip = ip,
            Serial = serial
        };
        _db.UserLoginHistory.Add(userLoginHistoryData);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserLoginHistoryData[]> Get(int userId, int limit = 10, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(Get));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
            activity.AddTag("limit", limit);
        }

        var query = _db.UserLoginHistory
            .TagWithSource(nameof(UserLoginHistoryRepository))
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.DateTime)
            .Take(limit);

        return await query.ToArrayAsync(cancellationToken);
    }

    public static readonly ActivitySource Activity = new("RealmCore.UserLoginHistoryRepository", "1.0.0");
}
