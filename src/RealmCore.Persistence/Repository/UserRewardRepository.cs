namespace RealmCore.Persistence.Repository;

internal sealed class UserRewardRepository : IUserRewardRepository
{
    private readonly IDb _db;

    public UserRewardRepository(IDb db)
    {
        _db = db;
    }

    public async Task<bool> TryAddReward(int userId, int rewardId)
    {
        try
        {
            _db.UserRewards.Add(new UserRewardData
            {
                RewardId = rewardId,
                UserId = userId
            });
            await _db.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
