
namespace RealmCore.Server.Interfaces;

public interface IUsersNotificationsService
{
    event Action<RealmPlayer, string, string, string?, int>? NotificationCreated;
    event Action<int>? NotificationRead;

    Task<int> CountUnread(RealmPlayer player);
    Task<int> Create(RealmPlayer player, string title, string description, string? excerpt = null);
    Task<List<UserNotificationData>> Get(RealmPlayer player, int limit = 10);
    Task<bool> MarkAsRead(int notificationId);
}
