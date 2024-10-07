namespace RealmCore.Server.Modules.Players.Notifications;

public sealed class NotificationsService
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly IServiceScope _serviceScope;
    private readonly IServiceProvider _serviceProvider;
    private readonly UserNotificationRepository _userNotificationRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly UsersInUse _usersInUse;

    public event Action<UserNotificationDto>? Created;

    public NotificationsService(IServiceProvider serviceProvider, IDateTimeProvider dateTimeProvider, UsersInUse usersInUse)
    {
        _serviceScope = serviceProvider.CreateScope();
        _serviceProvider = _serviceScope.ServiceProvider;
        _userNotificationRepository = _serviceProvider.GetRequiredService<UserNotificationRepository>();
        _dateTimeProvider = dateTimeProvider;
        _usersInUse = usersInUse;
    }

    public async Task<UserNotificationDto> Create(int userId, int type, string title, string content, string? excerpt = null, CancellationToken cancellationToken = default)
    {
        UserNotificationData? notificationData;
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            notificationData = await _userNotificationRepository.Create(userId, type, _dateTimeProvider.Now, title, content, excerpt, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }

        var userNotificationDto = UserNotificationDto.Map(notificationData);
        Created?.Invoke(userNotificationDto);
        return userNotificationDto;
    }
    
    public async Task<UserNotificationDto?> GetById(int id, CancellationToken cancellationToken = default)
    {
        UserNotificationData? notificationData;
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            notificationData = await _userNotificationRepository.GetById(id, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }

        if (notificationData == null)
            return null;

        var userNotificationDto = UserNotificationDto.Map(notificationData);
        return userNotificationDto;
    }
    
    public async Task<UserNotificationDto[]> Get(int userId, int skip = 0, int limit = 10, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var userNotifications = await _userNotificationRepository.Get(userId, skip, limit, cancellationToken);

            return userNotifications.Select(UserNotificationDto.Map).ToArray();
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

            return marked;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}