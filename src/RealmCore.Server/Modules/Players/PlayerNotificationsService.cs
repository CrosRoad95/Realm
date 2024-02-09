namespace RealmCore.Server.Modules.Players;

public interface IPlayerNotificationsService
{
    event Action<RealmPlayer, string, string, string?, int>? NotificationCreated;
    event Action<int>? NotificationRead;

    Task<int> CountUnread(RealmPlayer player, CancellationToken cancellationToken = default);
    Task<int> Create(RealmPlayer player, string title, string description, string? excerpt = null, CancellationToken cancellationToken = default);
    Task<List<UserNotificationData>> Get(RealmPlayer player, int limit = 10, CancellationToken cancellationToken = default);
    Task<bool> MarkAsRead(int notificationId, CancellationToken cancellationToken = default);
}

internal sealed class PlayerNotificationsService : IPlayerNotificationsService
{
    private readonly IUserNotificationRepository _userNotificationRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public event Action<RealmPlayer, string, string, string?, int>? NotificationCreated;
    public event Action<int>? NotificationRead;

    public PlayerNotificationsService(IUserNotificationRepository userNotificationRepository, IDateTimeProvider dateTimeProvider)
    {
        _userNotificationRepository = userNotificationRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<int> Create(RealmPlayer player, string title, string description, string? excerpt = null, CancellationToken cancellationToken = default)
    {
        var notification = await _userNotificationRepository.Create(player.UserId, _dateTimeProvider.Now, title, description, excerpt, cancellationToken);
        NotificationCreated?.Invoke(player, title, description, excerpt, notification.Id);
        return notification.Id;
    }

    public async Task<List<UserNotificationData>> Get(RealmPlayer player, int limit = 10, CancellationToken cancellationToken = default)
    {
        return await _userNotificationRepository.Get(player.UserId, limit, cancellationToken);
    }

    public async Task<int> CountUnread(RealmPlayer player, CancellationToken cancellationToken = default)
    {
        return await _userNotificationRepository.CountUnread(player.UserId, cancellationToken);
    }

    public async Task<bool> MarkAsRead(int notificationId, CancellationToken cancellationToken = default)
    {
        if (await _userNotificationRepository.MarkAsRead(notificationId, _dateTimeProvider.Now, cancellationToken))
        {
            NotificationRead?.Invoke(notificationId);
            return true;
        }
        return false;
    }
}
