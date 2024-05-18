namespace RealmCore.Server.Modules.Players.Notifications;

public interface IPlayersNotifications
{
    event Action<UserNotificationDto>? Created;
    event Action<UserNotificationDto>? Read;

    Task<int> CountUnread(RealmPlayer player, CancellationToken cancellationToken = default);
    Task<int> CountUnread(int userId, CancellationToken cancellationToken = default);
    Task<UserNotificationDto> Create(RealmPlayer player, string title, string description, string? excerpt = null, CancellationToken cancellationToken = default);
    Task<UserNotificationDto> Create(int userId, string title, string description, string? excerpt = null, CancellationToken cancellationToken = default);
    Task<List<UserNotificationData>> Get(RealmPlayer player, int limit = 10, CancellationToken cancellationToken = default);
    Task<List<UserNotificationData>> Get(int userId, int limit = 10, CancellationToken cancellationToken = default);
    Task<UserNotificationDto?> GetById(int notificationId, CancellationToken cancellationToken = default);
    Task<bool> TryMarkAsRead(int notificationId, CancellationToken cancellationToken = default);
}

internal sealed class PlayersNotifications : IPlayersNotifications
{
    private readonly IUserNotificationRepository _userNotificationRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUsersInUse _usersInUse;

    public event Action<UserNotificationDto>? Created;
    public event Action<UserNotificationDto>? Read;

    public PlayersNotifications(IUserNotificationRepository userNotificationRepository, IDateTimeProvider dateTimeProvider, IUsersInUse usersInUse)
    {
        _userNotificationRepository = userNotificationRepository;
        _dateTimeProvider = dateTimeProvider;
        _usersInUse = usersInUse;
    }

    public async Task<UserNotificationDto> Create(RealmPlayer player, string title, string description, string? excerpt = null, CancellationToken cancellationToken = default)
    {
        var notificationData = await _userNotificationRepository.Create(player.UserId, _dateTimeProvider.Now, title, description, excerpt, cancellationToken);
        player?.Notifications.RelayCreated(notificationData);
        var userNotificationDto = UserNotificationDto.Map(notificationData);
        Created?.Invoke(userNotificationDto);
        return userNotificationDto;
    }

    public async Task<UserNotificationDto> Create(int userId, string title, string description, string? excerpt = null, CancellationToken cancellationToken = default)
    {
        if(_usersInUse.TryGetPlayerByUserId(userId, out var player))
            return await Create(userId, title, description, excerpt, cancellationToken);

        var notificationData = await _userNotificationRepository.Create(userId, _dateTimeProvider.Now, title, description, excerpt, cancellationToken);
        var userNotificationDto = UserNotificationDto.Map(notificationData);
        Created?.Invoke(userNotificationDto);
        return userNotificationDto;
    }

    public async Task<List<UserNotificationData>> Get(RealmPlayer player, int limit = 10, CancellationToken cancellationToken = default)
    {
        return await _userNotificationRepository.Get(player.UserId, limit, cancellationToken);
    }

    public async Task<List<UserNotificationData>> Get(int userId, int limit = 10, CancellationToken cancellationToken = default)
    {
        return await _userNotificationRepository.Get(userId, limit, cancellationToken);
    }

    public async Task<int> CountUnread(RealmPlayer player, CancellationToken cancellationToken = default)
    {
        return await _userNotificationRepository.CountUnread(player.UserId, cancellationToken);
    }

    public async Task<int> CountUnread(int userId, CancellationToken cancellationToken = default)
    {
        return await _userNotificationRepository.CountUnread(userId, cancellationToken);
    }
    
    public async Task<UserNotificationDto?> GetById(int notificationId, CancellationToken cancellationToken = default)
    {
        var notificationData = await _userNotificationRepository.GetById(notificationId, cancellationToken);
        return UserNotificationDto.Map(notificationData);
    }

    public async Task<bool> TryMarkAsRead(int notificationId, CancellationToken cancellationToken = default)
    {
        if (await _userNotificationRepository.MarkAsRead(notificationId, _dateTimeProvider.Now, cancellationToken))
        {
            var notificationData = await _userNotificationRepository.GetById(notificationId, cancellationToken);
            if (notificationData == null)
                throw new InvalidOperationException();
            if (_usersInUse.TryGetPlayerByUserId(notificationData.UserId, out var player) && player != null)
                player.Notifications.RelayRead(notificationData);

            Read?.Invoke(UserNotificationDto.Map(notificationData));
            return true;
        }
        return false;
    }
}
