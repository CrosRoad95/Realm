namespace RealmCore.Server.Services.Players;

public interface IPlayerEventsService : IPlayerService, IEnumerable<UserEventDTO>
{
    event Action<IPlayerEventsService, IEnumerable<UserEventDTO>>? Added;

    void Add(int eventType, string? metadata = null);
    Task<List<UserEventDTO>> FetchMore(int count = 10, CancellationToken cancellationToken = default);
    IReadOnlyCollection<UserEventData> Get(IEnumerable<int>? events = null, int limit = 10);
}

internal class PlayerEventsService : IPlayerEventsService
{
    private readonly SemaphoreSlim _lock = new(1);
    private readonly IPlayerUserService _playerUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IDb _db;
    private ICollection<UserEventData> _userEventData = [];
    public event Action<IPlayerEventsService, IEnumerable<UserEventDTO>>? Added;
    public RealmPlayer Player { get; private set; }
    public PlayerEventsService(PlayerContext playerContext, IPlayerUserService playerUserService, IDateTimeProvider dateTimeProvider, IDb db)
    {
        Player = playerContext.Player;
        playerUserService.SignedIn += HandleSignedIn;
        playerUserService.SignedOut += HandleSignedOut;
        _playerUserService = playerUserService;
        _dateTimeProvider = dateTimeProvider;
        _db = db;
    }

    private void HandleSignedIn(IPlayerUserService playerUserService, RealmPlayer _)
    {
        _lock.Wait();
        try
        {
            _userEventData = playerUserService.User.Events;
        }
        finally
        {
            _lock.Release();
        }
    }

    private void HandleSignedOut(IPlayerUserService playerUserService, RealmPlayer _)
    {
        _lock.Wait();
        try
        {
            _userEventData = [];
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<List<UserEventDTO>> FetchMore(int count = 10, CancellationToken cancellationToken = default)
    {
        var last = _userEventData.LastOrDefault();
        if (last == null)
            return [];


        await _lock.WaitAsync(cancellationToken);
        try
        {
            var query = _db.UserEvents
                .Where(x => x.UserId == Player.UserId && x.Id < last.Id)
                .OrderByDescending(x => x.Id)
                .Take(count);

            var results = await query.ToListAsync(cancellationToken);

            return results.Select(UserEventDTO.Map).ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Add(int eventType, string? metadata = null)
    {
        var userEvent = new UserEventData
        {
            DateTime = _dateTimeProvider.Now,
            EventType = eventType,
            Metadata = metadata,
        };

        lock (_lock)
        {
            _userEventData.Add(userEvent);
        }
        _playerUserService.IncreaseVersion();
        Added?.Invoke(this, [UserEventDTO.Map(userEvent)]);
    }

    public IReadOnlyCollection<UserEventData> Get(IEnumerable<int>? events = null, int limit = 10)
    {
        lock (_lock)
        {
            var q = events != null ? _userEventData.Where(x => events.Contains(x.EventType)) : _userEventData;
            return q.Take(limit).ToList().AsReadOnly();
        }
    }

    public IEnumerator<UserEventDTO> GetEnumerator()
    {
        lock (_lock)
            return _userEventData.Select(UserEventDTO.Map).ToList().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
