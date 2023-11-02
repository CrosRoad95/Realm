namespace RealmCore.Server.Services;

internal sealed class UsersNotificationsService : IUsersNotificationsService
{
    private readonly IUserNotificationRepository _userNotificationRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public event Action<RealmPlayer, string, string, string?, int>? NotificationCreated;
    public event Action<int>? NotificationRead;

    public UsersNotificationsService(IUserNotificationRepository userNotificationRepository, IDateTimeProvider dateTimeProvider)
    {
        _userNotificationRepository = userNotificationRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<int> Create(RealmPlayer player, string title, string description, string? excerpt = null)
    {
        var notification = await _userNotificationRepository.Create(player.GetUserId(), _dateTimeProvider.Now, title, description, excerpt);
        NotificationCreated?.Invoke(player, title, description, excerpt, notification.Id);
        return notification.Id;
    }

    public async Task<List<UserNotificationData>> Get(RealmPlayer player, int limit = 10)
    {
        return await _userNotificationRepository.Get(player.GetUserId(), limit);
    }

    public async Task<int> CountUnread(RealmPlayer player)
    {
        return await _userNotificationRepository.CountUnread(player.GetUserId());
    }

    public async Task<bool> MarkAsRead(int notificationId)
    {
        if(await _userNotificationRepository.MarkAsRead(notificationId, _dateTimeProvider.Now))
        {
            NotificationRead?.Invoke(notificationId);
            return true;
        }
        return false;
    }
}
