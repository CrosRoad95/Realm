namespace RealmCore.Server.Modules.Players;

public interface IPlayerMoneyHistoryService
{
    Task Add(RealmPlayer player, decimal change, int? category = null, string? description = null, CancellationToken cancellationToken = default);
    Task<List<UserMoneyHistoryDTO>> Get(RealmPlayer player, int limit = 10, CancellationToken cancellationToken = default);
}

internal sealed class PlayerMoneyHistoryService : IPlayerMoneyHistoryService
{
    private readonly IUserMoneyHistoryRepository _userMoneyHistoryRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public PlayerMoneyHistoryService(IUserMoneyHistoryRepository userMoneyHistoryRepository, IDateTimeProvider dateTimeProvider)
    {
        _userMoneyHistoryRepository = userMoneyHistoryRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task Add(RealmPlayer player, decimal change, int? category = null, string? description = null, CancellationToken cancellationToken = default)
    {
        var money = player.Money.Amount;
        await _userMoneyHistoryRepository.Add(player.UserId, _dateTimeProvider.Now, money + change, change, category, description, cancellationToken);
    }

    public async Task<List<UserMoneyHistoryDTO>> Get(RealmPlayer player, int limit = 10, CancellationToken cancellationToken = default)
    {
        var moneyHistory = await _userMoneyHistoryRepository.Get(player.UserId, limit, cancellationToken);

        return moneyHistory.Select(UserMoneyHistoryDTO.Map).ToList();
    }
}
