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
            activity.AddTag("Serial", serial);
        }

        var query = CreateQueryBase()
            .AsNoTracking()
            .Where(x => x.RegisterSerial == serial);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }
    
    public async Task<UserData?> GetByDiscordUserId(ulong discordUserId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetByDiscordUserId));

        if (activity != null)
        {
            activity.AddTag("DiscordUserId", discordUserId);
        }

        var query = CreateQueryBase()
            .AsNoTracking()
            .Where(x => x.DiscordIntegration!.DiscordUserId == discordUserId);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<string?> GetLastNickName(int userId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetLastNickName));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
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
            activity.AddTag("UserId", userId);
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
            activity.AddTag("UserId", userId);
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
            activity.AddTag("UserName", userName);
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
            activity.AddTag("UserId", userId);
            activity.AddTag("Nick", nick);
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
            activity.AddTag("UserId", userId);
        }

        var query = CreateQueryBase()
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(x => x.Avatar);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> SetAvatar(int userId, string? avatar, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(SetAvatar));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
            activity.AddTag("Avatar", avatar);
        }

        var query = CreateQueryBase()
            .AsNoTracking()
            .Where(u => u.Id == userId);

        return await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.Avatar, avatar), cancellationToken) == 1;
    }
    
    public async Task<bool> SetSetting(int userId, int settingId, string? value, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(SetSetting));

        if(activity != null)
        {
            activity.AddTag("UserId", userId);
            activity.AddTag("SettingId", settingId);
        }

        var query = _db.UserSettings
            .TagWithSource(nameof(UsersRepository))
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.SettingId == settingId);

        return await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.Value, value), cancellationToken) == 1;
    }
    
    public async Task<string?> GetSetting(int userId, int settingId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetSetting));

        if(activity != null)
        {
            activity.AddTag("UserId", userId);
            activity.AddTag("SettingId", settingId);
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
