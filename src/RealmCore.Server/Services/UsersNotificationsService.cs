namespace RealmCore.Server.Services;

internal sealed class UsersNotificationsService : IUsersNotificationsService
{
    private readonly IUserNotificationRepository _userNotificationRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public event Action<Entity, string, string, string?, int>? NotificationCreated;
    public event Action<int>? NotificationRead;

    public UsersNotificationsService(IUserNotificationRepository userNotificationRepository, IDateTimeProvider dateTimeProvider)
    {
        _userNotificationRepository = userNotificationRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<int> Create(Entity entity, string title, string description, string? excerpt = null)
    {
        var userComponent = entity.GetRequiredComponent<UserComponent>();
        var notification = await _userNotificationRepository.Create(userComponent.Id, _dateTimeProvider.Now, title, description, excerpt);
        NotificationCreated?.Invoke(entity, title, description, excerpt, notification.Id);
        return notification.Id;
    }

    public async Task<List<UserNotificationData>> Get(Entity entity, int limit = 10)
    {
        var userComponent = entity.GetRequiredComponent<UserComponent>();
        return await _userNotificationRepository.Get(userComponent.Id, limit);
    }

    public async Task<int> CountUnread(Entity entity)
    {
        var userComponent = entity.GetRequiredComponent<UserComponent>();
        return await _userNotificationRepository.CountUnread(userComponent.Id);
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
