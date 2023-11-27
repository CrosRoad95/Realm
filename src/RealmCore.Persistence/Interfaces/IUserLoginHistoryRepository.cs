namespace RealmCore.Persistence.Interfaces;

public interface IUserLoginHistoryRepository
{
    Task Add(int userId, DateTime now, string ip, string serial, CancellationToken cancellationToken = default);
    Task<List<UserLoginHistoryData>> Get(int userId, int limit = 10, CancellationToken cancellationToken = default);
}
