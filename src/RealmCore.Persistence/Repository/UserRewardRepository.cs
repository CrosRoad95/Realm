namespace RealmCore.Persistence.Repository;

public sealed class UserRewardRepository
{
    private readonly IDb _db;

    public UserRewardRepository(IDb db)
    {
        _db = db;
    }

    public async Task<int[]> GetRewards(int userId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetRewards));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
        }

        var query = _db.UserRewards
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.RewardId);

        return await query.ToArrayAsync(cancellationToken);
    }

    public async Task<bool> TryAddReward(int userId, int rewardId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(TryAddReward));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
            activity.AddTag("RewardId", rewardId);
        }

        try
        {
            _db.UserRewards.Add(new UserRewardData
            {
                RewardId = rewardId,
                UserId = userId
            });
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static readonly ActivitySource Activity = new("RealmCore.UserRewardRepository", "1.0.0");
}
