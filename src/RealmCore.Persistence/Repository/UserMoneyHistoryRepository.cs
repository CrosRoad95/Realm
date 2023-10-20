namespace RealmCore.Persistence.Repository;

internal sealed class UserMoneyHistoryRepository : IUserMoneyHistoryRepository
{
    private readonly IDb _db;

    public UserMoneyHistoryRepository(IDb db)
    {
        _db = db;
    }

    public async Task<UserMoneyHistoryData> Add(int userId, DateTime now, decimal currentBalance, decimal amount, string? description = null)
    {
        var userMoneyHistoryData = new UserMoneyHistoryData
        {
            UserId = userId,
            DateTime = now,
            CurrentBalance = currentBalance,
            Amount = amount,
            Description = description
        };
        _db.UserMoneyHistory.Add(userMoneyHistoryData);
        await _db.SaveChangesAsync();
        return userMoneyHistoryData;
    }

    public async Task<List<UserMoneyHistoryData>> Get(int userId, int limit = 10)
    {
        var query = _db.UserMoneyHistory
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.DateTime)
            .Take(limit);

        return await query.ToListAsync();
    }
}
