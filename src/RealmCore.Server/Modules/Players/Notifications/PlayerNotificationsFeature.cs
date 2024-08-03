namespace RealmCore.Server.Modules.Players.Notifications;

public sealed class PlayerNotificationsFeature : IPlayerFeature, IEnumerable<UserNotificationDto>, IUsesUserPersistentData
{
    private readonly SemaphoreSlim _lock = new(1);
    private readonly IUserNotificationRepository _userNotificationRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private ICollection<UserNotificationData> _userNotificationDataCollection = [];

    public event Action<PlayerNotificationsFeature, UserNotificationDto>? Created;
    public event Action<PlayerNotificationsFeature, UserNotificationDto>? Read;
    public event Action? VersionIncreased;

    public RealmPlayer Player { get; }

    public PlayerNotificationsFeature(PlayerContext playerContext, IUserNotificationRepository userNotificationRepository, IDateTimeProvider dateTimeProvider, NotificationsService notificationsService)
    {
        Player = playerContext.Player;
        _userNotificationRepository = userNotificationRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public void LogIn(UserData userData)
    {
        _lock.Wait();
        try
        {
            _userNotificationDataCollection = userData.Notifications;
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Create(UserNotificationData data)
    {
        _userNotificationDataCollection.Add(data);

        var userNotificationDto = UserNotificationDto.Map(data);
        Created?.Invoke(this, userNotificationDto);
        VersionIncreased?.Invoke();
    }
    
    public void Update(UserNotificationData data)
    {
        var notification = _userNotificationDataCollection.Where(x => x.Id == data.Id).FirstOrDefault();
        if(notification != null)
        {
            if(notification.ReadTime == null && data.ReadTime != null)
            {
                notification.ReadTime = data.ReadTime;
                Read?.Invoke(this, UserNotificationDto.Map(data));
            }
        }
    }

    public async Task<UserNotificationDto?> GetById(int notificationId, CancellationToken cancellationToken = default)
    {
        var notificationData = await _userNotificationRepository.GetById(notificationId, cancellationToken);
        return UserNotificationDto.Map(notificationData);
    }

    public async Task<UserNotificationDto[]> FetchMore(int number = 10, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var last = _userNotificationDataCollection.LastOrDefault();
            if (last == null)
                return [];

            var results = await _userNotificationRepository.FetchMore(Player.UserId, last.Id, number, cancellationToken);

            return results.Select(UserNotificationDto.Map).ToArray();
        }
        finally
        {
            _lock.Release();
        }
    }

    public IEnumerator<UserNotificationDto> GetEnumerator()
    {
        UserNotificationData[] view;
        _lock.Wait();
        try
        {
            view = [.. _userNotificationDataCollection];
        }
        finally
        {
            _lock.Release();
        }

        foreach (var notificationData in view)
        {
            yield return UserNotificationDto.Map(notificationData);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
