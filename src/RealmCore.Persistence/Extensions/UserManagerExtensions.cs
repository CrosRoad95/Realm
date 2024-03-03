namespace RealmCore.Persistence.Extensions;

public static class UserManagerExtensions
{
    public static async Task<UserData?> GetUserBySerial(this UserManager<UserData> userManager, string serial, CancellationToken cancellationToken = default)
    {
        var query = userManager.Users
            .AsNoTracking()
            .TagWithSource(nameof(UserManagerExtensions))
            .Where(x => x.RegisterSerial == serial);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public static async Task<int?> GetUserIdBySerial(this UserManager<UserData> userManager, string serial, CancellationToken cancellationToken = default)
    {
        var query = userManager.Users
            .AsNoTracking()
            .TagWithSource(nameof(UserManagerExtensions))
            .Where(x => x.RegisterSerial == serial)
            .Select(x => x.Id);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public static async Task<UserData?> GetReadOnlyUserById(this UserManager<UserData> userManager, int id, CancellationToken cancellationToken = default)
    {
        var query = userManager.Users
            .AsNoTracking()
            .TagWithSource(nameof(UserManagerExtensions))
            .Where(x => x.Id == id);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public static async Task<UserData?> GetUserById(this UserManager<UserData> userManager, int id, CancellationToken cancellationToken = default)
    {
        var query = userManager.Users
            .TagWithSource(nameof(UserManagerExtensions))
            .Where(x => x.Id == id);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public static async Task<string?> GetUserNameById(this UserManager<UserData> userManager, int id, CancellationToken cancellationToken = default)
    {
        var query = userManager.Users
            .AsNoTracking()
            .TagWithSource(nameof(UserManagerExtensions))
            .Where(x => x.Id == id)
            .Select(x => x.UserName);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public static async Task DisableUser(this UserManager<UserData> userManager, int userId, CancellationToken cancellationToken = default)
    {
        var query = userManager.Users
            .AsNoTracking()
            .TagWithSource(nameof(UserManagerExtensions))
            .Where(x => x.Id == userId);
        await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.IsDisabled, true), cancellationToken);
    }

    public static async Task SetQuickLoginEnabled(this UserManager<UserData> userManager, int userId, bool enabled, CancellationToken cancellationToken = default)
    {
        var query = userManager.Users
            .AsNoTracking()
            .TagWithSource(nameof(UserManagerExtensions))
            .Where(x => x.Id == userId);
        await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.QuickLogin, enabled), cancellationToken);
    }

    public static async Task<bool> IsQuickLoginEnabledBySerial(this UserManager<UserData> userManager, string serial, CancellationToken cancellationToken = default)
    {
        var query = userManager.Users
            .AsNoTracking()
            .TagWithSource(nameof(UserManagerExtensions))
            .Where(x => x.RegisterSerial == serial)
            .Select(x => x.QuickLogin);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public static async Task<bool> IsQuickLoginEnabledById(this UserManager<UserData> userManager, int id, CancellationToken cancellationToken = default)
    {
        var query = userManager.Users
            .AsNoTracking()
            .TagWithSource(nameof(UserManagerExtensions))
            .Where(x => x.Id == id)
            .Select(x => x.QuickLogin);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public static async Task<UserData?> GetUserByLoginCaseInsensitive(this UserManager<UserData> userManager, string login, DateTime now, CancellationToken cancellationToken = default)
    {
        var query = userManager.Users
            .TagWithSource(nameof(UserManagerExtensions))
            //.IncludeAll(now)
            .Where(u => u.NormalizedUserName == login.ToUpper());
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public static async Task<int> CountUsersBySerial(this UserManager<UserData> userManager, string serial, CancellationToken cancellationToken = default)
    {
        var query = userManager.Users
            .TagWithSource(nameof(UserManagerExtensions))
            .Where(u => u.RegisterSerial == serial);
        return await query.CountAsync(cancellationToken);
    }

    public static async Task<List<UserData>> GetUsersBySerial(this UserManager<UserData> userManager, string serial, DateTime now, CancellationToken cancellationToken = default)
    {
        var query = userManager.Users
            .AsNoTracking()
            .TagWithSource(nameof(UserManagerExtensions))
            .IncludeAll(now)
            .Where(u => u.RegisterSerial == serial);
        return await query.ToListAsync(cancellationToken);
    }

    public static async Task<bool> IsUserNameInUse(this UserManager<UserData> userManager, string userName, CancellationToken cancellationToken = default)
    {
        var query = userManager.Users
            .AsNoTracking()
            .TagWithSource(nameof(UserManagerExtensions))
            .Where(x => x.UserName == userName);
        return await query.AnyAsync(cancellationToken);
    }

    public static async Task<bool> IsUserNameInUseCaseInsensitive(this UserManager<UserData> userManager, string userName, CancellationToken cancellationToken = default)
    {
        var query = userManager.Users
            .AsNoTracking()
            .TagWithSource(nameof(UserManagerExtensions))
            .Where(x => x.NormalizedUserName == userName.ToUpper());
        return await query.AnyAsync(cancellationToken);
    }

    public static async Task<bool> UpdateLastNewsReadDateTime(this UserManager<UserData> userManager, int userId, DateTime now, CancellationToken cancellationToken = default)
    {
        var query = userManager.Users
            .TagWithSource(nameof(UserManagerExtensions))
            .Where(x => x.Id == userId);
        var user = await query.FirstOrDefaultAsync(cancellationToken);
        if (user == null)
            return false;

        user.LastNewsReadDateTime = now;
        await userManager.UpdateAsync(user);
        return true;
    }

    public static async Task<string?> GetLastNickName(this UserManager<UserData> userManager, int userId, CancellationToken cancellationToken = default)
    {
        var query = userManager.Users
            .AsNoTracking()
            .TagWithSource(nameof(UserManagerExtensions))
            .Where(x => x.Id == userId);
        var user = await query.FirstOrDefaultAsync(cancellationToken);
        return user?.Nick;
    }

    public static async Task<int> CountBySerial(this UserManager<UserData> userManager, string serial, CancellationToken cancellationToken = default)
    {
        var query = userManager.Users
            .AsNoTracking()
            .TagWithSource(nameof(UserManagerExtensions))
            .Where(x => x.RegisterSerial == serial);
        return await query.CountAsync(cancellationToken);
    }
}
