namespace RealmCore.Server.Interfaces;

public interface IRewardService
{
    Task<bool> TryGiveReward(RealmPlayer player, int rewardId, CancellationToken cancellationToken = default);
}
