namespace RealmCore.Persistence.Interfaces;

public interface IUserEventRepository
{
    Task AddEvent(int userId, int eventType, DateTime dateTime, string? metadata = null, CancellationToken cancellationToken = default);
    Task<List<UserEventData>> GetAllEventsByUserId(int userId, IEnumerable<int>? events = null, CancellationToken cancellationToken = default);
    Task<List<UserEventData>> GetLastEventsByUserId(int userId, int limit = 10, IEnumerable<int>? events = null, CancellationToken cancellationToken = default);
}