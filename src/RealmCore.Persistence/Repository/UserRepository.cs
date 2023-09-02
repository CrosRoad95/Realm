namespace RealmCore.Persistence.Repository;

internal sealed class UserRepository : IUserRepository
{
    private readonly IDb _db;
    private readonly UserManager<UserData> _userManager;

    public UserRepository(IDb db, UserManager<UserData> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<UserData?> GetUserBySerial(string serial)
    {
        var query = _db.Users
            .AsNoTracking()
            .TagWith(nameof(UserRepository))
            .Where(x => !x.IsDisabled)
            .Where(x => x.RegisterSerial == serial);
        return await query.FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<string?> GetUserNameById(int id)
    {
        var query = _db.Users
            .AsNoTracking()
            .TagWith(nameof(UserRepository))
            .Where(x => !x.IsDisabled)
            .Where(x => x.Id == id)
            .Select(x => x.UserName);
        return await query.FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<int> GetUserIdBySerial(string serial)
    {
        var query = _db.Users
            .AsNoTracking()
            .TagWith(nameof(UserRepository))
            .Where(x => !x.IsDisabled)
            .Where(x => x.RegisterSerial == serial)
            .Select(x => x.Id);
        return await query.FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<string?> GetUserBySerial(int id)
    {
        var query = _db.Users
            .AsNoTracking()
            .TagWith(nameof(UserRepository))
            .Where(x => !x.IsDisabled)
            .Where(x => x.Id == id)
            .Select(x => x.UserName);

        return await query.FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<Dictionary<int, string?>> GetUserNamesByIds(IEnumerable<int> ids)
    {
        var query = _db.Users
            .AsNoTracking()
            .TagWith(nameof(UserRepository))
            .Where(x => !x.IsDisabled)
            .Where(x => ids.Contains(x.Id));
        return await query.ToDictionaryAsync(x => x.Id, x => x.UserName).ConfigureAwait(false);
    }

    public async Task DisableUser(int userId)
    {
        var query = _db.Users
            .AsNoTracking()
            .TagWith(nameof(UserRepository))
            .Where(x => x.Id == userId);
        await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.IsDisabled, true)).ConfigureAwait(false);
    }


    public async Task SetQuickLoginEnabled(int userId, bool enabled)
    {
        var query = _db.Users
            .AsNoTracking()
            .TagWith(nameof(UserRepository))
            .Where(x => !x.IsDisabled)
            .Where(x => x.Id == userId);
        await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.QuickLogin, enabled)).ConfigureAwait(false);
    }

    public async Task<bool> IsQuickLoginEnabledBySerial(string serial)
    {
        var query = _db.Users
            .AsNoTracking()
            .TagWith(nameof(UserRepository))
            .Where(x => !x.IsDisabled)
            .Where(x => x.RegisterSerial == serial)
            .Select(x => x.QuickLogin);
        return await query.FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<bool> IsQuickLoginEnabledById(int id)
    {
        var query = _db.Users
            .AsNoTracking()
            .TagWith(nameof(UserRepository))
            .Where(x => !x.IsDisabled)
            .Where(x => x.Id == id)
            .Select(x => x.QuickLogin);
        return await query.FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<UserData?> GetUserByLogin(string login)
    {
        var query = _userManager.Users
            .AsNoTracking()
            .TagWith(nameof(UserRepository))
            .IncludeAll()
            .Where(u => u.UserName == login);
        return await query.FirstOrDefaultAsync();
    }

    public async Task<UserData?> GetUserByLoginCaseInsensitive(string login)
    {
        var query = _userManager.Users
            .AsNoTracking()
            .TagWith(nameof(UserRepository))
            .IncludeAll()
            .Where(u => u.NormalizedUserName == login.ToUpper());
        return await query.FirstOrDefaultAsync();
    }

    public async Task<int> CountUsersBySerial(string serial)
    {
        var query = _userManager.Users
            .TagWith(nameof(UserRepository))
            .Where(u => u.RegisterSerial == serial);
        return await query.CountAsync().ConfigureAwait(false);
    }

    public async Task<List<UserData>> GetUsersBySerial(string serial)
    {
        var query = _userManager.Users
            .AsNoTracking()
            .TagWith(nameof(UserRepository))
            .IncludeAll()
            .Where(u => u.RegisterSerial == serial);
        return await query.ToListAsync().ConfigureAwait(false);
    }

    public async Task<UserData?> GetUserById(int id)
    {
        var query = _userManager.Users
            .AsNoTracking()
            .TagWith(nameof(UserRepository))
            .IncludeAll()
            .Where(u => u.Id == id);

        return await query.FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<bool> IsUserNameInUse(string userName)
    {
        var query = _userManager.Users
            .AsNoTracking()
            .Where(x => x.UserName == userName);
        return await query.AnyAsync().ConfigureAwait(false);
    }

    public async Task<bool> IsUserNameInUseCaseInsensitive(string userName)
    {
        var query = _userManager.Users
            .AsNoTracking()
            .TagWith(nameof(UserRepository))
            .Where(x => x.NormalizedUserName == userName.ToUpper());
        return await query.AnyAsync().ConfigureAwait(false);
    }

    public async Task<bool> TryUpdateLastNickName(int userId, string nick)
    {
        var query = _db.Users
            .AsNoTracking()
            .TagWith(nameof(UserRepository))
            .Where(x => x.Id == userId);
        return await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.Nick, nick)).ConfigureAwait(false) == 1;
    }
}
