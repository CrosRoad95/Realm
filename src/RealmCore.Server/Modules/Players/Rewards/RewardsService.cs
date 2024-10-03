namespace RealmCore.Server.Modules.Players.Rewards;

public sealed class RewardsService
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly IServiceScope _serviceScope;
    private readonly IServiceProvider _serviceProvider;
    private readonly UserRewardRepository _userRewardRepository;

    public RewardsService(IServiceProvider serviceProvider)
    {
        _serviceScope = serviceProvider.CreateScope();
        _serviceProvider = _serviceScope.ServiceProvider;
        _userRewardRepository = _serviceProvider.GetRequiredService<UserRewardRepository>();
    }

    public async Task<int[]> GetRewards(RealmPlayer player, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _userRewardRepository.GetRewards(player.UserId, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
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
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _userRewardRepository.TryAddReward(player.UserId, rewardId, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> TryGiveReward(int userId, int rewardId, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _userRewardRepository.TryAddReward(userId, rewardId, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
