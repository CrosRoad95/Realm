namespace RealmCore.Persistence.Repository;

public interface IUserNotificationRepository
{
    Task<int> CountUnread(int userId, CancellationToken cancellationToken = default);
    Task<UserNotificationData> Create(int userId, DateTime now, string title, string content, string? excerpt = null, CancellationToken cancellationToken = default);
    Task<List<UserNotificationData>> Get(int userId, int limit = 10, CancellationToken cancellationToken = default);
    Task<UserNotificationData?> GetById(int notificationId, CancellationToken cancellationToken = default);
    Task<bool> MarkAsRead(int id, DateTime now, CancellationToken cancellationToken = default);
}

internal sealed class UserNotificationRepository : IUserNotificationRepository
{
    private readonly IDb _db;

    public UserNotificationRepository(IDb db)
    {
        _db = db;
    }

    public async Task<List<UserNotificationData>> Get(int userId, int limit = 10, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(Get));

        var query = _db.UserNotifications
            .TagWithSource(nameof(UserNotificationRepository))
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.SentTime)
            .Take(limit);

        return await query.ToListAsync(cancellationToken);
    }
    
    public async Task<UserNotificationData?> GetById(int notificationId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetById));

        var query = _db.UserNotifications
            .TagWithSource(nameof(UserNotificationRepository))
            .AsNoTracking()
            .Where(x => x.Id == notificationId);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
    
    public async Task<UserNotificationData> Create(int userId, DateTime now, string title, string content, string? excerpt = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(Create));

        var userNotificationData = new UserNotificationData
        {
            UserId = userId,
            SentTime = now,
            Title = title,
            Content = content,
            Excerpt = excerpt ?? (content.Length > 250 ? content[..250] : content)
        };
        _db.UserNotifications.Add(userNotificationData);
        await _db.SaveChangesAsync(cancellationToken);
        return userNotificationData;
    }

    public async Task<int> CountUnread(int userId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(CountUnread));

        var query = _db.UserNotifications
            .TagWithSource(nameof(UserNotificationRepository))
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.ReadTime == null);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<bool> MarkAsRead(int id, DateTime now, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(MarkAsRead));

        var notification = await _db.UserNotifications
            .TagWithSource(nameof(UserNotificationRepository))
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        if (notification == null || notification.ReadTime != null)
            return false;

        notification.ReadTime = now;
        return await _db.SaveChangesAsync(cancellationToken) == 1;
    }

    public static readonly ActivitySource Activity = new("RealmCore.UserNotificationRepository", "1.0.0");
}
