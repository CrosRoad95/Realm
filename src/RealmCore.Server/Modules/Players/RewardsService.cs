namespace RealmCore.Server.Modules.Players;

public sealed class RewardsService
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly IServiceScope _serviceScope;
    private readonly IServiceProvider _serviceProvider;
    private readonly IUserRewardRepository _userRewardRepository;

    public RewardsService(IServiceProvider serviceProvider)
    {
        _serviceScope = serviceProvider.CreateScope();
        _serviceProvider = _serviceScope.ServiceProvider;
        _userRewardRepository = _serviceProvider.GetRequiredService<IUserRewardRepository>();
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
}
