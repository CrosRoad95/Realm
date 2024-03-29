﻿namespace RealmCore.Server.Modules.Players;

public interface IPlayerEventsFeature : IPlayerFeature, IEnumerable<UserEventDto>
{
    event Action<IPlayerEventsFeature, IEnumerable<UserEventDto>>? Added;

    void Add(int eventType, string? metadata = null);
    Task<List<UserEventDto>> FetchMore(int count = 10, CancellationToken cancellationToken = default);
    IReadOnlyCollection<UserEventData> Get(IEnumerable<int>? events = null, int limit = 10);
}

internal class PlayerEventsFeature : IPlayerEventsFeature, IUsesUserPersistentData
{
    private readonly SemaphoreSlim _lock = new(1);
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IDb _db;
    private ICollection<UserEventData> _userEventData = [];
    public event Action<IPlayerEventsFeature, IEnumerable<UserEventDto>>? Added;
    public event Action? VersionIncreased;

    public RealmPlayer Player { get; init; }
    public PlayerEventsFeature(PlayerContext playerContext, IDateTimeProvider dateTimeProvider, IDb db)
    {
        Player = playerContext.Player;
        _dateTimeProvider = dateTimeProvider;
        _db = db;
    }

    public void SignIn(UserData userData)
    {
        _lock.Wait();
        try
        {
            _userEventData = userData.Events;
        }
        finally
        {
            _lock.Release();
        }
    }

    public void SignOut()
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

    public async Task<List<UserEventDto>> FetchMore(int count = 10, CancellationToken cancellationToken = default)
    {
        var last = _userEventData.LastOrDefault();
        if (last == null)
            return [];


        await _lock.WaitAsync(cancellationToken);
        try
        {
            var query = _db.UserEvents
                .Where(x => x.UserId == Player.PersistentId && x.Id < last.Id)
                .OrderByDescending(x => x.Id)
                .Take(count);

            var results = await query.ToListAsync(cancellationToken);

            return results.Select(UserEventDto.Map).ToList();
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
        VersionIncreased?.Invoke();
        Added?.Invoke(this, [UserEventDto.Map(userEvent)]);
    }

    public IReadOnlyCollection<UserEventData> Get(IEnumerable<int>? events = null, int limit = 10)
    {
        lock (_lock)
        {
            var q = events != null ? _userEventData.Where(x => events.Contains(x.EventType)) : _userEventData;
            return q.Take(limit).ToList().AsReadOnly();
        }
    }

    public IEnumerator<UserEventDto> GetEnumerator()
    {
        lock (_lock)
            return _userEventData.Select(UserEventDto.Map).ToList().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Dispose()
    {
        lock (_lock)
            _userEventData = [];
    }
}
