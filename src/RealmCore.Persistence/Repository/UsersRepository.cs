namespace RealmCore.Persistence.Repository;

public sealed class UsersRepository
{
    private readonly IDb _db;

    public UsersRepository(IDb db)
    {
        _db = db;
    }

    public async Task<UserData?> GetBySerial(string serial, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetBySerial));

        if (activity != null)
        {
            activity.SetTag("serial", serial);
        }

        var query = CreateQueryBase()
            .AsNoTracking()
            .Where(x => x.RegisterSerial == serial);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<string?> GetLastNickName(int userId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetLastNickName));

        if (activity != null)
        {
            activity.SetTag("userId", userId);
        }

        var query = CreateQueryBase()
            .AsNoTracking()
            .Where(x => x.Id == userId);
        var user = await query.FirstOrDefaultAsync(cancellationToken);
        return user?.Nick;
    }

    public async Task<string[]> GetRoles(int userId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetRoles));

        if (activity != null)
        {
            activity.SetTag("userId", userId);
        }

        var subQuery = _db.UserRoles
            .AsNoTracking()
            .TagWithSource(nameof(UsersRepository))
            .Where(x => x.UserId == userId)
            .Select(x => x.RoleId);

        var query = _db.Roles.Where(x => subQuery.Contains(x.Id))
            .AsNoTracking()
            .TagWithSource(nameof(UsersRepository))
            .Select(x => x.Name);

        var roles = await query.ToArrayAsync(cancellationToken);
        return roles;
    }

    public async Task<UserData?> GetUserByUserName(string userName, DateTime dateTime, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetUserByUserName));

        if (activity != null)
        {
            activity.SetTag("userName", userName);
        }

        var query = CreateQueryBase()
            .AsNoTrackingWithIdentityResolution()
            .IncludeAll(dateTime)
            .Where(u => u.UserName == userName);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> TryUpdateLastNickname(int userId, string nick, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(TryUpdateLastNickname));

        if(activity != null)
        {
            activity.SetTag("userId", userId);
            activity.SetTag("nick", nick);
        }

        var query = CreateQueryBase()
            .AsNoTracking()
            .Where(u => u.Id == userId);

        return await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.Nick, nick), cancellationToken) == 1;
    }

    private IQueryable<UserData> CreateQueryBase() => _db.Users
        .TagWithSource(nameof(UsersRepository));

    public static readonly ActivitySource Activity = new("RealmCore.UsersRepository", "1.0.0");
}
