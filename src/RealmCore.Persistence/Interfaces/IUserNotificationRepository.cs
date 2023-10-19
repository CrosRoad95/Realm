namespace RealmCore.Persistence.Interfaces;

public interface IUserNotificationRepository
{
    Task<int> CountUnread(int userId);
    Task<UserNotificationData> Create(int userId, DateTime now, string title, string content, string? excerpt = null);
    Task<List<UserNotificationData>> Get(int userId, int limit = 10);
    Task<bool> MarkAsRead(int id, DateTime now);
}
