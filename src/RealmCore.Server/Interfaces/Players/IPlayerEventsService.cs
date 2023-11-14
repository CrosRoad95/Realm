namespace RealmCore.Server.Interfaces.Players;

public interface IPlayerEventsService : IPlayerService, IEnumerable<UserEventDTO>
{
    event Action<IPlayerEventsService, IEnumerable<UserEventDTO>>? Added;

    void Add(int eventType, string? metadata = null);
    Task<List<UserEventDTO>> FetchMore(int count = 10, CancellationToken cancellationToken = default);
    IReadOnlyCollection<UserEventData> Get(IEnumerable<int>? events = null, int limit = 10);
}
