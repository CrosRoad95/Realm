namespace RealmCore.Persistence.Interfaces;

public interface IUserMoneyHistoryRepository
{
    Task<UserMoneyHistoryData> Add(int userId, DateTime now, decimal currentBalance, decimal amount, string? description = null);
    Task<List<UserMoneyHistoryData>> Get(int userId, int limit = 10);
}
