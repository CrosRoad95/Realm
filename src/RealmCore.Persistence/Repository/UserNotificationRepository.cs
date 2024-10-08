﻿namespace RealmCore.Persistence.Repository;

public sealed class UserNotificationRepository
{
    private readonly IDb _db;

    public UserNotificationRepository(IDb db)
    {
        _db = db;
    }

    public async Task<UserNotificationData[]> Get(int userId, int skip = 0, int limit = 10, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(Get));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
            activity.AddTag("Limit", limit);
        }

        var query = _db.UserNotifications
            .TagWithSource(nameof(UserNotificationRepository))
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.SentTime)
            .Skip(skip)
            .Take(limit);

        return await query.ToArrayAsync(cancellationToken);
    }
    
    public async Task<UserNotificationData?> GetById(int id, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetById));

        if (activity != null)
        {
            activity.AddTag("Id", id);
        }

        var query = _db.UserNotifications
            .TagWithSource(nameof(UserNotificationRepository))
            .AsNoTracking()
            .Where(x => x.Id == id);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserNotificationData> Create(int userId, int type, DateTime now, string title, string content, string? excerpt = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(Create));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
            activity.AddTag("Type", type);
            activity.AddTag("Title", title);
            activity.AddTag("Content", content);
            activity.AddTag("Excerpt", excerpt);
        }

        var userNotificationData = new UserNotificationData
        {
            UserId = userId,
            Type = type,
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

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
        }

        var query = _db.UserNotifications
            .TagWithSource(nameof(UserNotificationRepository))
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.ReadTime == null);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<bool> MarkAsRead(int id, DateTime now, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(MarkAsRead));

        if (activity != null)
        {
            activity.AddTag("Id", id);
        }

        var query = _db.UserNotifications
            .TagWithSource(nameof(UserNotificationRepository))
            .Where(x => x.Id == id && x.ReadTime == null);

        return await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.ReadTime, now), cancellationToken) == 1;
    }

    public static readonly ActivitySource Activity = new("RealmCore.UserNotificationRepository", "1.0.0");
}
