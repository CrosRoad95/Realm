namespace RealmCore.Persistence.Repository;

public interface IUserLoginHistoryRepository
{
    Task Add(int userId, DateTime now, string ip, string serial, CancellationToken cancellationToken = default);
    Task<List<UserLoginHistoryData>> Get(int userId, int limit = 10, CancellationToken cancellationToken = default);
}

internal sealed class UserLoginHistoryRepository : IUserLoginHistoryRepository
{
    private readonly IDb _db;

    public UserLoginHistoryRepository(IDb db)
    {
        _db = db;
    }

    public async Task Add(int userId, DateTime now, string ip, string serial, CancellationToken cancellationToken = default)
    {
        var userLoginHistoryData = new UserLoginHistoryData
        {
            UserId = userId,
            DateTime = now,
            Ip = ip,
            Serial = serial
        };
        _db.UserLoginHistory.Add(userLoginHistoryData);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<UserLoginHistoryData>> Get(int userId, int limit = 10, CancellationToken cancellationToken = default)
    {
        var query = _db.UserLoginHistory
            .TagWithSource(nameof(UserMoneyHistoryRepository))
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.DateTime)
            .Take(limit);

        return await query.ToListAsync(cancellationToken);
    }
}
