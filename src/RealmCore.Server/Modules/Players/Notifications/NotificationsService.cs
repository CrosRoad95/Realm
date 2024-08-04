namespace RealmCore.Server.Modules.Players.Notifications;

public sealed class NotificationsService
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly IServiceScope _serviceScope;
    private readonly IServiceProvider _serviceProvider;
    private readonly UserNotificationRepository _userNotificationRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly UsersInUse _usersInUse;

    public NotificationsService(IServiceProvider serviceProvider, IDateTimeProvider dateTimeProvider, UsersInUse usersInUse)
    {
        _serviceScope = serviceProvider.CreateScope();
        _serviceProvider = _serviceScope.ServiceProvider;
        _userNotificationRepository = _serviceProvider.GetRequiredService<UserNotificationRepository>();
        _dateTimeProvider = dateTimeProvider;
        _usersInUse = usersInUse;
    }

    public async Task<UserNotificationDto> Create(int userId, string title, string description, string? excerpt = null, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var notificationData = await _userNotificationRepository.Create(userId, _dateTimeProvider.Now, title, description, excerpt, cancellationToken);

            if (_usersInUse.TryGetPlayerByUserId(userId, out var player))
                player.Notifications.Create(notificationData);

            var userNotificationDto = UserNotificationDto.Map(notificationData);
            return userNotificationDto;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> TryMarkAsRead(int notificationId, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var now = _dateTimeProvider.Now;
            var marked = await _userNotificationRepository.MarkAsRead(notificationId, now, cancellationToken);
            var notificationData = await _userNotificationRepository.GetById(notificationId, cancellationToken);

            if (notificationData != null && _usersInUse.TryGetPlayerByUserId(notificationData.UserId, out var player))
                player.Notifications.Update(notificationData);

            return marked;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}