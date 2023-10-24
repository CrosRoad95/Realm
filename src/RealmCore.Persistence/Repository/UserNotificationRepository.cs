namespace RealmCore.Persistence.Repository;

internal sealed class UserNotificationRepository : IUserNotificationRepository
{
    private readonly IDb _db;

    public UserNotificationRepository(IDb db)
    {
        _db = db;
    }

    public async Task<List<UserNotificationData>> Get(int userId, int limit = 10, CancellationToken cancellationToken = default)
    {
        var query = _db.UserNotifications
            .TagWithSource(nameof(UserNotificationRepository))
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.SentTime)
            .Take(limit);

        return await query.ToListAsync();
    }
    
    public async Task<UserNotificationData> Create(int userId, DateTime now, string title, string content, string? excerpt = null)
    {
        var userNotificationData = new UserNotificationData
        {
            UserId = userId,
            SentTime = now,
            Title = title,
            Content = content,
            Excerpt = excerpt ?? (content.Length > 250 ? content[..250] : content)
        };
        _db.UserNotifications.Add(userNotificationData);
        await _db.SaveChangesAsync();
        return userNotificationData;
    }

    public async Task<int> CountUnread(int userId)
    {
        var query = _db.UserNotifications
            .TagWithSource(nameof(UserNotificationRepository))
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.ReadTime == null);

        return await query.CountAsync();
    }

    public async Task<bool> MarkAsRead(int id, DateTime now)
    {
        var notification = await _db.UserNotifications
            .TagWithSource(nameof(UserNotificationRepository))
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();

        if (notification == null || notification.ReadTime != null)
            return false;

        notification.ReadTime = now;
        return await _db.SaveChangesAsync() == 1;
    }
}
