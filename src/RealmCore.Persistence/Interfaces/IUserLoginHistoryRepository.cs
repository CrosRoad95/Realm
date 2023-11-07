namespace RealmCore.Persistence.Interfaces;

public interface IUserLoginHistoryRepository
{
    void Add(int userId, DateTime now, string ip, string serial);
    Task<List<UserLoginHistoryData>> Get(int userId, int limit = 10, CancellationToken cancellationToken = default);
}
