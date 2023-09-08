namespace RealmCore.Persistence.Interfaces;

public interface IUserRewardRepository
{
    Task<bool> TryAddReward(int userId, int rewardId, CancellationToken cancellationToken = default);
}
