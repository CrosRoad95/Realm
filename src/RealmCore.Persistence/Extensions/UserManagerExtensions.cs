namespace RealmCore.Persistence.Extensions;

public static class UserManagerExtensions
{
    public static async Task<UserData?> GetUserBySerial(this UserManager<UserData> userManager, string serial, CancellationToken cancellationToken = default)
    {
        var query = userManager.Users
            .AsNoTracking()
            .TagWith(nameof(UserManagerExtensions))
            .Where(x => x.RegisterSerial == serial);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public static async Task<int?> GetUserIdBySerial(this UserManager<UserData> userManager, string serial, CancellationToken cancellationToken = default)
    {
        var query = userManager.Users
            .AsNoTracking()
            .TagWith(nameof(UserManagerExtensions))
            .Where(x => x.RegisterSerial == serial)
            .Select(x => x.Id);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }


    public static async Task<string?> GetUserById(this UserManager<UserData> userManager, int id, CancellationToken cancellationToken = default)
    {
        var query = userManager.Users
            .AsNoTracking()
            .TagWith(nameof(UserManagerExtensions))
            .Where(x => x.Id == id)
            .Select(x => x.UserName);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public static async Task DisableUser(this UserManager<UserData> userManager, int userId, CancellationToken cancellationToken = default)
    {
        var query = userManager.Users
            .AsNoTracking()
            .TagWith(nameof(UserManagerExtensions))
            .Where(x => x.Id == userId);
        await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.IsDisabled, true), cancellationToken);
    }

    public static async Task SetQuickLoginEnabled(this UserManager<UserData> userManager, int userId, bool enabled, CancellationToken cancellationToken = default)
    {
        var query = userManager.Users
            .AsNoTracking()
            .TagWith(nameof(UserManagerExtensions))
            .Where(x => x.Id == userId);
        await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.QuickLogin, enabled), cancellationToken);
    }

    public static async Task<bool> IsQuickLoginEnabledBySerial(this UserManager<UserData> userManager, string serial, CancellationToken cancellationToken = default)
    {
        var query = userManager.Users
            .AsNoTracking()
            .TagWith(nameof(UserManagerExtensions))
            .Where(x => x.RegisterSerial == serial)
            .Select(x => x.QuickLogin);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public static async Task<bool> IsQuickLoginEnabledById(this UserManager<UserData> userManager, int id, CancellationToken cancellationToken = default)
    {
        var query = userManager.Users
            .AsNoTracking()
            .TagWith(nameof(UserManagerExtensions))
            .Where(x => x.Id == id)
            .Select(x => x.QuickLogin);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public static async Task<UserData?> GetUserByLogin(this UserManager<UserData> userManager, string login, CancellationToken cancellationToken = default)
    {
        var query = userManager.Users
            .AsNoTracking()
            .TagWith(nameof(UserManagerExtensions))
            .IncludeAll()
            .Where(u => u.UserName == login);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public static async Task<UserData?> GetUserByLoginCaseInsensitive(this UserManager<UserData> userManager, string login, CancellationToken cancellationToken = default)
    {
        var query = userManager.Users
            .AsNoTracking()
            .TagWith(nameof(UserManagerExtensions))
            .IncludeAll()
            .Where(u => u.NormalizedUserName == login.ToUpper());
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public static async Task<int> CountUsersBySerial(this UserManager<UserData> userManager, string serial, CancellationToken cancellationToken = default)
    {
        var query = userManager.Users
            .TagWith(nameof(UserManagerExtensions))
            .Where(u => u.RegisterSerial == serial);
        return await query.CountAsync(cancellationToken);
    }

    public static async Task<List<UserData>> GetUsersBySerial(this UserManager<UserData> userManager, string serial, CancellationToken cancellationToken = default)
    {
        var query = userManager.Users
            .AsNoTracking()
            .TagWith(nameof(UserManagerExtensions))
            .IncludeAll()
            .Where(u => u.RegisterSerial == serial);
        return await query.ToListAsync(cancellationToken);
    }

    public static async Task<bool> IsUserNameInUse(this UserManager<UserData> userManager, string userName, CancellationToken cancellationToken = default)
    {
        var query = userManager.Users
            .AsNoTracking()
            .Where(x => x.UserName == userName);
        return await query.AnyAsync(cancellationToken);
    }

    public static async Task<bool> IsUserNameInUseCaseInsensitive(this UserManager<UserData> userManager, string userName, CancellationToken cancellationToken = default)
    {
        var query = userManager.Users
            .AsNoTracking()
            .TagWith(nameof(UserManagerExtensions))
            .Where(x => x.NormalizedUserName == userName.ToUpper());
        return await query.AnyAsync(cancellationToken);
    }

    public static async Task<bool> TryUpdateLastNickName(this UserManager<UserData> userManager, int userId, string nick, CancellationToken cancellationToken = default)
    {
        var query = userManager.Users
            .TagWith(nameof(UserManagerExtensions))
            .Where(x => x.Id == userId);
        var user = await query.FirstOrDefaultAsync();
        if (user == null)
            return false;

        user.Nick = nick;
        await userManager.UpdateAsync(user);
        return true;
        //return await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.Nick, nick), cancellationToken) == 1;
    }

    public static async Task<bool> UpdateLastNewsReadDateTime(this UserManager<UserData> userManager, int userId, DateTime now, CancellationToken cancellationToken = default)
    {
        var query = userManager.Users
            .TagWith(nameof(UserManagerExtensions))
            .Where(x => x.Id == userId);
        var user = await query.FirstOrDefaultAsync();
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
            .TagWith(nameof(UserManagerExtensions))
            .Where(x => x.Id == userId);
        var user = await query.FirstOrDefaultAsync(cancellationToken);
        return user?.Nick;
    }

    public static async Task<int> CountBySerial(this UserManager<UserData> userManager, string serial, CancellationToken cancellationToken = default)
    {
        var query = userManager.Users
            .AsNoTracking()
            .TagWith(nameof(UserManagerExtensions))
            .Where(x => x.RegisterSerial == serial);
        return await query.CountAsync(cancellationToken);
    }
}
