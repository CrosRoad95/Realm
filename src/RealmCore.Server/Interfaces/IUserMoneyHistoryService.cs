namespace RealmCore.Server.Interfaces;

public interface IUserMoneyHistoryService
{
    Task Add(RealmPlayer player, decimal change, int? category = null, string? description = null, CancellationToken cancellationToken = default);
    Task<List<UserMoneyHistoryDTO>> Get(RealmPlayer player, int limit = 10, CancellationToken cancellationToken = default);
}
