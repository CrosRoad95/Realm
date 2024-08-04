namespace RealmCore.Persistence.Repository;

public sealed class FriendRepository
{
    private readonly IDb _db;

    public FriendRepository(IDb db)
    {
        _db = db;
    }

    public async Task CreateFriend(int userId1, int userId2, DateTime now)
    {
        using var activity = Activity.StartActivity(nameof(CreateFriend));

        if(activity != null)
        {
            activity.AddTag("UserId1", userId1);
            activity.AddTag("UserId2", userId2);
            activity.AddTag("Now", now);
        }

        var friendData = new FriendData
        {
            UserId1 = userId1,
            UserId2 = userId2,
            CreatedAt = now
        };

        _db.Friends.Add(friendData);

        await _db.SaveChangesAsync();

        _db.Entry(friendData).State = EntityState.Detached;
    }

    public async Task CreatePendingRequest(int userId1, int userId2, DateTime now)
    {
        using var activity = Activity.StartActivity(nameof(CreatePendingRequest));

        if (activity != null)
        {
            activity.AddTag("UserId1", userId1);
            activity.AddTag("UserId2", userId2);
            activity.AddTag("Now", now);
        }

        var pendingFriendRequestData = new PendingFriendRequestData
        {
            UserId1 = userId1,
            UserId2 = userId2,
            CreatedAt = now
        };

        _db.PendingFriendsRequests.Add(pendingFriendRequestData);

        await _db.SaveChangesAsync();

        _db.Entry(pendingFriendRequestData).State = EntityState.Detached;
    }

    public async Task<int[]> GetByUserId(int userId)
    {
        using var activity = Activity.StartActivity(nameof(GetByUserId));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
        }

        var query = _db.Friends
            .TagWithSource(nameof(FriendRepository))
            .AsNoTracking()
            .Where(x => x.UserId1 == userId || x.UserId2 == userId)
            .Select(x => new { x.UserId1, x.UserId2 });

        return (await query.ToArrayAsync()).SelectMany(x => new int[] { x.UserId1, x.UserId2 }).Distinct().Where(x => x != userId).ToArray();
    }

    public async Task<int[]> GetPendingIncomingFriendsRequests(int userId)
    {
        using var activity = Activity.StartActivity(nameof(GetPendingIncomingFriendsRequests));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
        }

        var query = _db.PendingFriendsRequests
            .TagWithSource(nameof(FriendRepository))
            .AsNoTracking()
            .Where(x => x.UserId2 == userId)
            .Select(x => x.UserId1);

        return (await query.ToArrayAsync()).Distinct().Where(x => x != userId).ToArray();
    }
    
    public async Task<int[]> GetPendingOutgoingFriendsRequests(int userId)
    {
        using var activity = Activity.StartActivity(nameof(GetPendingIncomingFriendsRequests));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
        }

        var query = _db.PendingFriendsRequests
            .TagWithSource(nameof(FriendRepository))
            .AsNoTracking()
            .Where(x => x.UserId1 == userId)
            .Select(x => x.UserId2);

        return (await query.ToArrayAsync()).Distinct().Where(x => x != userId).ToArray();
    }

    public async Task<bool> RemoveFriend(int userId1, int userId2)
    {
        using var activity = Activity.StartActivity(nameof(RemoveFriend));

        if (activity != null)
        {
            activity.AddTag("UserId1", userId1);
            activity.AddTag("UserId2", userId2);
        }

        var query = _db.Friends
            .TagWithSource(nameof(FriendRepository))
            .AsNoTracking()
            .Where(x => (x.UserId1 == userId1 && x.UserId2 == userId2) || (x.UserId1 == userId2 && x.UserId2 == userId1));

        return await query.ExecuteDeleteAsync() == 1;
    }

    public async Task<bool> RemovePendingRequest(int userId1, int userId2)
    {
        using var activity = Activity.StartActivity(nameof(RemovePendingRequest));

        if (activity != null)
        {
            activity.AddTag("UserId1", userId1);
            activity.AddTag("UserId2", userId2);
        }

        var query = _db.PendingFriendsRequests
            .TagWithSource(nameof(FriendRepository))
            .AsNoTracking()
            .Where(x => x.UserId1 == userId1 && x.UserId2 == userId2);

        return await query.ExecuteDeleteAsync() == 1;
    }

    public async Task<bool> AreFriends(int userId1, int userId2)
    {
        var query = _db.Friends
            .TagWithSource(nameof(FriendRepository))
            .AsNoTracking()
            .Where(x => (x.UserId1 == userId1 && x.UserId2 == userId2) || (x.UserId1 == userId2 && x.UserId2 == userId1));

        return await query.AnyAsync();
    }
    
    public async Task<bool> IsPendingFriendRequest(int userId1, int userId2)
    {
        var query = _db.PendingFriendsRequests
            .TagWithSource(nameof(FriendRepository))
            .AsNoTracking()
            .Where(x => (x.UserId1 == userId1 && x.UserId2 == userId2) || (x.UserId1 == userId2 && x.UserId2 == userId1));

        return await query.AnyAsync();
    }

    public static readonly ActivitySource Activity = new("RealmCore.FriendRepository", "1.0.0");
}
