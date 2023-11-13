using System.Collections;

namespace RealmCore.Server.Services.Players;

internal class PlayerEventsService : IPlayerEventsService
{
    private readonly object _lock = new();
    private readonly IPlayerUserService _playerUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private ICollection<UserEventData> _userEventData = [];
    public event Action<IPlayerEventsService, UserEventDTO>? Added;
    public RealmPlayer Player { get; private set; }
    public PlayerEventsService(PlayerContext playerContext, IPlayerUserService playerUserService, IDateTimeProvider dateTimeProvider)
    {
        Player = playerContext.Player;
        playerUserService.SignedIn += HandleSignedIn;
        playerUserService.SignedOut += HandleSignedOut;
        _playerUserService = playerUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    private void HandleSignedIn(IPlayerUserService playerUserService, RealmPlayer _)
    {
        lock (_lock)
            _userEventData = playerUserService.User.Events;
    }

    private void HandleSignedOut(IPlayerUserService playerUserService, RealmPlayer _)
    {
        lock (_lock)
            _userEventData = [];
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
        Added?.Invoke(this, UserEventDTO.Map(userEvent));
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
