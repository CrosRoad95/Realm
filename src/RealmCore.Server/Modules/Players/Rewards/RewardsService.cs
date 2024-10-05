namespace RealmCore.Server.Modules.Players.Rewards;

public sealed class RewardsService
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly IServiceScope _serviceScope;
    private readonly IServiceProvider _serviceProvider;
    private readonly UserRewardRepository _userRewardRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public event Action<int, int>? RewardGiven;

    public RewardsService(IServiceProvider serviceProvider, IDateTimeProvider dateTimeProvider)
    {
        _serviceScope = serviceProvider.CreateScope();
        _serviceProvider = _serviceScope.ServiceProvider;
        _userRewardRepository = _serviceProvider.GetRequiredService<UserRewardRepository>();
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<int[]> GetRewards(RealmPlayer player, CancellationToken cancellationToken = default)
    {
        return await GetRewards(player.UserId, cancellationToken);
    }
    
    public async Task<int[]> GetRewards(int userId, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _userRewardRepository.GetRewards(userId, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> TryGiveReward(RealmPlayer player, int rewardId, CancellationToken cancellationToken = default)
    {
        return await TryGiveReward(player.UserId, rewardId, cancellationToken);
    }

    public async Task<bool> TryGiveReward(int userId, int rewardId, CancellationToken cancellationToken = default)
    {
        bool given = false;
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            given = await _userRewardRepository.TryAddReward(userId, rewardId, _dateTimeProvider.Now, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
        if (given)
            RewardGiven?.Invoke(userId, rewardId);
        return given;
    }
}
