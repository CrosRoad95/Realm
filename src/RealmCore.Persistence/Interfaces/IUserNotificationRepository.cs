namespace RealmCore.Persistence.Interfaces;

public interface IUserNotificationRepository
{
    Task<int> CountUnread(int userId, CancellationToken cancellationToken = default);
    Task<UserNotificationData> Create(int userId, DateTime now, string title, string content, string? excerpt = null, CancellationToken cancellationToken = default);
    Task<List<UserNotificationData>> Get(int userId, int limit = 10, CancellationToken cancellationToken = default);
    Task<bool> MarkAsRead(int id, DateTime now, CancellationToken cancellationToken = default);
}
