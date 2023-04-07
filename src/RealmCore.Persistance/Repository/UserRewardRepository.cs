namespace RealmCore.Persistance.Repository;

internal class UserRewardRepository : IUserRewardRepository
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
            await Commit();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public Task Commit()
    {
        return _db.SaveChangesAsync();
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
