namespace RealmCore.Persistence.Interfaces;

public interface IUserMoneyHistoryRepository
{
    Task<UserMoneyHistoryData> Add(int userId, DateTime now, decimal currentBalance, decimal amount, int? category = null, string? description = null, CancellationToken cancellationToken = default);
    Task<List<UserMoneyHistoryData>> Get(int userId, int limit = 10, CancellationToken cancellationToken = default);
}
