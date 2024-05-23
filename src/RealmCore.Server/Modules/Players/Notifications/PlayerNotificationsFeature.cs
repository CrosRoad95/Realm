namespace RealmCore.Server.Modules.Players.Notifications;

public interface IPlayerNotificationsFeature : IPlayerFeature, IEnumerable<UserNotificationDto>
{
    event Action<IPlayerNotificationsFeature, UserNotificationDto>? Created;
    event Action<IPlayerNotificationsFeature, UserNotificationDto>? Read;

    Task<UserNotificationDto[]> FetchMore(int count = 10, CancellationToken cancellationToken = default);
    void RelayCreated(UserNotificationData userNotificationData);
    void RelayRead(UserNotificationData userNotificationData);
}

internal sealed class PlayerNotificationsFeature : IPlayerNotificationsFeature, IUsesUserPersistentData
{
    private readonly SemaphoreSlim _lock = new(1);
    private readonly IDb _db;

    private ICollection<UserNotificationData> _userNotificationDataList = [];

    public event Action<IPlayerNotificationsFeature, UserNotificationDto>? Created;
    public event Action<IPlayerNotificationsFeature, UserNotificationDto>? Read;
    public event Action? VersionIncreased;

    public RealmPlayer Player { get; }

    public PlayerNotificationsFeature(PlayerContext playerContext, IDb db)
    {
        Player = playerContext.Player;
        _db = db;
    }

    public void LogIn(UserData userData)
    {
        _lock.Wait();
        try
        {
            _userNotificationDataList = userData.Notifications;
        }
        finally
        {
            _lock.Release();
        }
    }

    public void LogOut()
    {
        _lock.Wait();
        try
        {
            _userNotificationDataList = [];
        }
        finally
        {
            _lock.Release();
        }
    }


    public void RelayCreated(UserNotificationData userNotificationData)
    {
        _lock.Wait();
        try
        {
            _userNotificationDataList.Add(userNotificationData);
            Created?.Invoke(this, UserNotificationDto.Map(userNotificationData));
        }
        finally
        {
            _lock.Release();
        }
    }

    public void RelayRead(UserNotificationData userNotificationData)
    {
        _lock.Wait();
        try
        {
            Read?.Invoke(this, UserNotificationDto.Map(userNotificationData));
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<UserNotificationDto[]> FetchMore(int count = 10, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var last = _userNotificationDataList.LastOrDefault();
            if (last == null)
                return [];

            var query = _db.UserNotifications
                .Where(x => x.UserId == Player.UserId && x.Id < last.Id)
                .OrderByDescending(x => x.Id)
                .Take(count);

            var results = await query.ToListAsync(cancellationToken);

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
            view = [.. _userNotificationDataList];
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
