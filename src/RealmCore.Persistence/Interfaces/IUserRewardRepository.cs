namespace RealmCore.Persistence.Interfaces;

public interface IUserRewardRepository : IRepositoryBase
{
    Task<bool> TryAddReward(int userId, int rewardId);
}
