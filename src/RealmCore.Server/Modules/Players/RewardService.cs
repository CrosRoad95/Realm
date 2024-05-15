namespace RealmCore.Server.Modules.Players;

public interface IRewardService
{
    Task<bool> TryGiveReward(RealmPlayer player, int rewardId, CancellationToken cancellationToken = default);
}

internal sealed class RewardService : IRewardService
{
    public RewardService()
    {

    }

    public async Task<bool> TryGiveReward(RealmPlayer player, int rewardId, CancellationToken cancellationToken = default)
    {
        var userRewardRepository = player.GetRequiredService<IUserRewardRepository>();
        return await userRewardRepository.TryAddReward(player.PersistentId, rewardId, cancellationToken);
    }
}
