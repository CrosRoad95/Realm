namespace RealmCore.Server.Modules.Players.Notifications;

public interface IPlayerNotificationsFeature : IPlayerFeature, IEnumerable<UserNotificationDto>
{
    event Action<IPlayerNotificationsFeature, UserNotificationDto>? Created;
    event Action<IPlayerNotificationsFeature, UserNotificationDto>? Read;

    Task<List<UserNotificationDto>> FetchMore(int count = 10, CancellationToken cancellationToken = default);
    void RelayCreated(UserNotificationData userNotificationData);
    void RelayRead(UserNotificationData userNotificationData);
}

internal sealed class PlayerNotificationsFeature : IPlayerNotificationsFeature, IDisposable
{
    private readonly SemaphoreSlim _lock = new(1);
    private readonly IPlayerUserFeature _playerUserFeature;
    private readonly IDb _db;

    private ICollection<UserNotificationData> _userNotificationDataList = [];

    public event Action<IPlayerNotificationsFeature, UserNotificationDto>? Created;
    public event Action<IPlayerNotificationsFeature, UserNotificationDto>? Read;

    public RealmPlayer Player { get; }

    public PlayerNotificationsFeature(PlayerContext playerContext, IPlayerUserFeature playerUserFeature, IDb db)
    {
        Player = playerContext.Player;
        playerUserFeature.SignedIn += HandleSignedIn;
        playerUserFeature.SignedOut += HandleSignedOut;
        _playerUserFeature = playerUserFeature;
        _db = db;
    }

    private void HandleSignedIn(IPlayerUserFeature playerUserFeature, RealmPlayer _)
    {
        _lock.Wait();
        try
        {
            _userNotificationDataList = playerUserFeature.UserData.Notifications;
        }
        finally
        {
            _lock.Release();
        }
    }

    private void HandleSignedOut(IPlayerUserFeature playerUserFeature, RealmPlayer _)
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

    public async Task<List<UserNotificationDto>> FetchMore(int count = 10, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var last = _userNotificationDataList.LastOrDefault();
            if (last == null)
                return [];

            var query = _db.UserNotifications
                .Where(x => x.UserId == Player.PersistentId && x.Id < last.Id)
                .OrderByDescending(x => x.Id)
                .Take(count);

            var results = await query.ToListAsync(cancellationToken);

            return results.Select(UserNotificationDto.Map).ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public IEnumerator<UserNotificationDto> GetEnumerator()
    {
        _lock.Wait();
        try
        {
            return new List<UserNotificationDto>(_userNotificationDataList.Select(UserNotificationDto.Map)).GetEnumerator();
        }
        finally
        {
            _lock.Release();
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Dispose()
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
}
