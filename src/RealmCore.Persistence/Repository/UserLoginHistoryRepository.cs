namespace RealmCore.Persistence.Repository;

internal sealed class UserLoginHistoryRepository : IUserLoginHistoryRepository
{
    private readonly IDb _db;

    public UserLoginHistoryRepository(IDb db)
    {
        _db = db;
    }

    public async Task<UserLoginHistoryData> Add(int userId, DateTime now, string ip, string serial)
    {
        var userLoginHistoryData = new UserLoginHistoryData
        {
            UserId = userId,
            DateTime = now,
            Ip = ip,
            Serial = serial
        };
        _db.UserLoginHistory.Add(userLoginHistoryData);
        await _db.SaveChangesAsync();
        return userLoginHistoryData;
    }

    public async Task<List<UserLoginHistoryData>> Get(int userId, int limit = 10)
    {
        var query = _db.UserLoginHistory
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.DateTime)
            .Take(limit);

        return await query.ToListAsync();
    }
}
