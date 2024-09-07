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

    public async Task<UserData?> GetUserById(int userId, DateTime dateTime, bool includeAll = true, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetUserByUserName));

        if (activity != null)
        {
            activity.SetTag("UserId", userId);
        }

        var query = CreateQueryBase()
            .AsNoTrackingWithIdentityResolution()
            .Where(u => u.Id == userId);

        if (includeAll)
            query = query.IncludeAll(dateTime);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
    
    public async Task<UserData?> GetUserByUserName(string userName, DateTime dateTime, bool includeAll = true, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetUserByUserName));

        if (activity != null)
        {
            activity.SetTag("UserName", userName);
        }

        var query = CreateQueryBase()
            .AsNoTrackingWithIdentityResolution()
            .Where(u => u.UserName == userName);

        if (includeAll)
            query = query.IncludeAll(dateTime);

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
    
    public async Task<string?> GetAvatar(int userId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetAvatar));

        if(activity != null)
        {
            activity.SetTag("UserId", userId);
        }

        var query = CreateQueryBase()
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(x => x.Avatar);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
    
    public async Task<string?> GetSetting(int userId, int settingId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetSetting));

        if(activity != null)
        {
            activity.SetTag("UserId", userId);
            activity.SetTag("SettingId", settingId);
        }

        var query = _db.UserSettings
            .TagWithSource(nameof(UsersRepository))
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.SettingId == settingId)
            .Select(x => x.Value);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    private IQueryable<UserData> CreateQueryBase() => _db.Users
        .TagWithSource(nameof(UsersRepository));

    public static readonly ActivitySource Activity = new("RealmCore.UsersRepository", "1.0.0");
}
