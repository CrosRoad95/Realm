namespace RealmCore.Server.Modules.Players.Money;

public interface IMoneyHistoryService
{
    Task Add(RealmPlayer player, decimal change, int? category = null, string? description = null, CancellationToken cancellationToken = default);
    Task Add(RealmPlayer player, decimal amount, decimal change, int? category = null, string? description = null, CancellationToken cancellationToken = default);
    Task<UserMoneyHistoryDto[]> Get(RealmPlayer player, int limit = 10, CancellationToken cancellationToken = default);
}

internal sealed class MoneyHistoryService : IMoneyHistoryService
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public MoneyHistoryService(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task Add(RealmPlayer player, decimal change, int? category = null, string? description = null, CancellationToken cancellationToken = default)
    {
        var userMoneyHistoryRepository = player.GetRequiredService<IUserMoneyHistoryRepository>();
        var amount = player.Money.Amount;
        await userMoneyHistoryRepository.Add(player.UserId, _dateTimeProvider.Now, amount + change, change, category, description, cancellationToken);
    }
    
    public async Task Add(RealmPlayer player, decimal amount, decimal change, int? category = null, string? description = null, CancellationToken cancellationToken = default)
    {
        var userMoneyHistoryRepository = player.GetRequiredService<IUserMoneyHistoryRepository>();
        await userMoneyHistoryRepository.Add(player.UserId, _dateTimeProvider.Now, amount + change, change, category, description, cancellationToken);
    }

    public async Task<UserMoneyHistoryDto[]> Get(RealmPlayer player, int limit = 10, CancellationToken cancellationToken = default)
    {
        var userMoneyHistoryRepository = player.GetRequiredService<IUserMoneyHistoryRepository>();
        var moneyHistory = await userMoneyHistoryRepository.Get(player.UserId, limit, cancellationToken);

        return [.. moneyHistory.Select(UserMoneyHistoryDto.Map)];
    }
}
