
namespace RealmCore.Server.Interfaces;

public interface IUsersNotificationsService
{
    event Action<Entity, string, string, string?, int>? NotificationCreated;
    event Action<int>? NotificationRead;

    Task<int> CountUnread(Entity entity);
    Task<int> Create(Entity entity, string title, string description, string? excerpt = null);
    Task<List<UserNotificationData>> Get(Entity entity, int limit = 10);
    Task<bool> MarkAsRead(int notificationId);
}
