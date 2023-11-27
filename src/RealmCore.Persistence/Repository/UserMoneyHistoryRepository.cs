namespace RealmCore.Persistence.Repository;

internal sealed class UserMoneyHistoryRepository : IUserMoneyHistoryRepository
{
    private readonly IDb _db;

    public UserMoneyHistoryRepository(IDb db)
    {
        _db = db;
    }

    public async Task<UserMoneyHistoryData> Add(int userId, DateTime now, decimal currentBalance, decimal amount, int? category = null, string? description = null, CancellationToken cancellationToken = default)
    {
        var userMoneyHistoryData = new UserMoneyHistoryData
        {
            UserId = userId,
            DateTime = now,
            CurrentBalance = currentBalance,
            Amount = amount,
            Category = category,
            Description = description
        };
        _db.UserMoneyHistory.Add(userMoneyHistoryData);
        await _db.SaveChangesAsync(cancellationToken);
        return userMoneyHistoryData;
    }

    public async Task<List<UserMoneyHistoryData>> Get(int userId, int limit = 10, CancellationToken cancellationToken = default)
    {
        var query = _db.UserMoneyHistory
            .TagWithSource(nameof(UserMoneyHistoryRepository))
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.DateTime)
            .Take(limit);

        return await query.ToListAsync(cancellationToken);
    }
}
