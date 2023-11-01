

namespace RealmCore.Server.Interfaces;

public interface IUserMoneyHistoryService
{
    Task Add(RealmPlayer player, decimal change, int? category = null, string? description = null);
    Task<List<UserMoneyHistoryDTO>> Get(RealmPlayer player, int limit = 10);
}
