
namespace RealmCore.Server.Interfaces.Players;

public interface IPlayerEventsService : IPlayerService, IEnumerable<UserEventDTO>
{
    void Add(int eventType, string? metadata = null);
    IReadOnlyCollection<UserEventData> Get(IEnumerable<int>? events = null, int limit = 10);
}
