

namespace RealmCore.Server.Interfaces;

public interface IUserMoneyHistoryService
{
    Task Add(Entity entity, decimal change, int? category = null, string? description = null);
    Task<List<UserMoneyHistoryDTO>> Get(Entity entity, int limit = 10);
}
