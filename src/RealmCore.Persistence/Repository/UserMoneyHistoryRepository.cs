namespace RealmCore.Persistence.Repository;

public interface IUserMoneyHistoryRepository
{
    Task<UserMoneyHistoryData> Add(int userId, DateTime now, decimal currentBalance, decimal amount, int? category = null, string? description = null, CancellationToken cancellationToken = default);
    Task<UserMoneyHistoryData[]> Get(int userId, int limit = 10, CancellationToken cancellationToken = default);
}

internal sealed class UserMoneyHistoryRepository : IUserMoneyHistoryRepository
{
    private readonly IDb _db;

    public UserMoneyHistoryRepository(IDb db)
    {
        _db = db;
    }

    public async Task<UserMoneyHistoryData> Add(int userId, DateTime now, decimal currentBalance, decimal amount, int? category = null, string? description = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(Add));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
            activity.AddTag("Now", now);
            activity.AddTag("CurrentBalance", currentBalance);
            activity.AddTag("Amount", amount);
            activity.AddTag("Category", category);
            activity.AddTag("Description", description);
        }

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

    public async Task<UserMoneyHistoryData[]> Get(int userId, int limit = 10, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(Get));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
            activity.AddTag("Limit", limit);
        }

        var query = _db.UserMoneyHistory
            .TagWithSource(nameof(UserMoneyHistoryRepository))
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.DateTime)
            .Take(limit);

        return await query.ToArrayAsync(cancellationToken);
    }

    public static readonly ActivitySource Activity = new("RealmCore.UserMoneyHistoryRepository", "1.0.0");
}
