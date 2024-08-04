namespace RealmCore.Server.Modules.Players.Money;

public sealed class MoneyHistoryService
{
    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IServiceScope _serviceScope;
    private readonly UserMoneyHistoryRepository _userMoneyHistoryRepository;

    public MoneyHistoryService(IDateTimeProvider dateTimeProvider, IServiceProvider serviceProvider)
    {
        _dateTimeProvider = dateTimeProvider;
        _serviceScope = serviceProvider.CreateScope();
        _userMoneyHistoryRepository = _serviceScope.ServiceProvider.GetRequiredService<UserMoneyHistoryRepository>();
    }

    public async Task Add(RealmPlayer player, decimal change, int? category = null, string? description = null, CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            var amount = player.Money.Amount;
            await _userMoneyHistoryRepository.Add(player.UserId, _dateTimeProvider.Now, amount + change, change, category, description, cancellationToken);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public async Task Add(RealmPlayer player, decimal amount, decimal change, int? category = null, string? description = null, CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            await _userMoneyHistoryRepository.Add(player.UserId, _dateTimeProvider.Now, amount + change, change, category, description, cancellationToken);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public async Task<UserMoneyHistoryDto[]> Get(RealmPlayer player, int limit = 10, CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            var moneyHistory = await _userMoneyHistoryRepository.Get(player.UserId, limit, cancellationToken);

            return [.. moneyHistory.Select(UserMoneyHistoryDto.Map)];
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
}
