namespace RealmCore.Server.Modules.Players.Notifications;

public interface IPlayersNotifications
{
    Task<UserNotificationDto> Create(int userId, string title, string description, string? excerpt = null, CancellationToken cancellationToken = default);
    Task<bool> TryMarkAsRead(int notificationId, CancellationToken cancellationToken = default);
}

internal sealed class PlayersNotifications : IPlayersNotifications
{
    private readonly IServiceScope _serviceScope;
    private readonly IServiceProvider _serviceProvider;
    private readonly IUserNotificationRepository _userNotificationRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUsersInUse _usersInUse;

    public PlayersNotifications(IServiceProvider serviceProvider, IDateTimeProvider dateTimeProvider, IUsersInUse usersInUse)
    {
        _serviceScope = serviceProvider.CreateScope();
        _serviceProvider = _serviceScope.ServiceProvider;
        _userNotificationRepository = _serviceProvider.GetRequiredService<IUserNotificationRepository>();
        _dateTimeProvider = dateTimeProvider;
        _usersInUse = usersInUse;
    }

    public async Task<UserNotificationDto> Create(int userId, string title, string description, string? excerpt = null, CancellationToken cancellationToken = default)
    {
        var notificationData = await _userNotificationRepository.Create(userId, _dateTimeProvider.Now, title, description, excerpt, cancellationToken);

        if (_usersInUse.TryGetPlayerByUserId(userId, out var player))
            player.Notifications.Create(notificationData);

        var userNotificationDto = UserNotificationDto.Map(notificationData);
        return userNotificationDto;
    }

    public async Task<bool> TryMarkAsRead(int notificationId, CancellationToken cancellationToken = default)
    {
        var now = _dateTimeProvider.Now;
        var marked = await _userNotificationRepository.MarkAsRead(notificationId, now, cancellationToken);
        var notificationData = await _userNotificationRepository.GetById(notificationId, cancellationToken);

        if (notificationData != null && _usersInUse.TryGetPlayerByUserId(notificationData.UserId, out var player))
            player.Notifications.Update(notificationData);

        return marked;
    }
}