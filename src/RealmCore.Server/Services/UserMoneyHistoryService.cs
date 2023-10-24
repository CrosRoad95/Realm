namespace RealmCore.Server.Services;

internal sealed class UserMoneyHistoryService : IUserMoneyHistoryService
{
    private readonly IUserMoneyHistoryRepository _userMoneyHistoryRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UserMoneyHistoryService(IUserMoneyHistoryRepository userMoneyHistoryRepository, IDateTimeProvider dateTimeProvider)
    {
        _userMoneyHistoryRepository = userMoneyHistoryRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task Add(Entity entity, decimal change, int? category = null, string? description = null)
    {
        var userComponent = entity.GetRequiredComponent<UserComponent>();
        var moneyComponent = entity.GetRequiredComponent<MoneyComponent>();
        var money = moneyComponent.Money;
        await _userMoneyHistoryRepository.Add(userComponent.Id, _dateTimeProvider.Now, money + change, change, category, description);
    }

    public async Task<List<UserMoneyHistoryDTO>> Get(Entity entity, int limit = 10)
    {
        var moneyHistory = await _userMoneyHistoryRepository.Get(entity.GetRequiredComponent<UserComponent>().Id, limit);

        return moneyHistory.Select(x => new UserMoneyHistoryDTO
        {
            Id = x.Id,
            DateTime = x.DateTime,
            Amount = x.Amount,
            CurrentBalance = x.CurrentBalance,
            Category = x.Category,
            Description = x.Description
        }).ToList();
    }
}
