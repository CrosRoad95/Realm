namespace RealmCore.Server.Interfaces;

public interface IUsersNotificationsService
{
    event Action<RealmPlayer, string, string, string?, int>? NotificationCreated;
    event Action<int>? NotificationRead;

    Task<int> CountUnread(RealmPlayer player, CancellationToken cancellationToken = default);
    Task<int> Create(RealmPlayer player, string title, string description, string? excerpt = null, CancellationToken cancellationToken = default);
    Task<List<UserNotificationData>> Get(RealmPlayer player, int limit = 10, CancellationToken cancellationToken = default);
    Task<bool> MarkAsRead(int notificationId, CancellationToken cancellationToken = default);
}
