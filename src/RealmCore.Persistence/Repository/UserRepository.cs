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

    public async Task<UserData?> GetUserBySerial(string serial, CancellationToken cancellationToken = default)
    {
        var query = _db.Users
            .AsNoTracking()
            .TagWith(nameof(UserRepository))
            .Where(x => !x.IsDisabled)
            .Where(x => x.RegisterSerial == serial);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<string?> GetUserNameById(int id, CancellationToken cancellationToken = default)
    {
        var query = _db.Users
            .AsNoTracking()
            .TagWith(nameof(UserRepository))
            .Where(x => !x.IsDisabled)
            .Where(x => x.Id == id)
            .Select(x => x.UserName);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> GetUserIdBySerial(string serial, CancellationToken cancellationToken = default)
    {
        var query = _db.Users
            .AsNoTracking()
            .TagWith(nameof(UserRepository))
            .Where(x => !x.IsDisabled)
            .Where(x => x.RegisterSerial == serial)
            .Select(x => x.Id);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<string?> GetUserBySerial(int id, CancellationToken cancellationToken = default)
    {
        var query = _db.Users
            .AsNoTracking()
            .TagWith(nameof(UserRepository))
            .Where(x => !x.IsDisabled)
            .Where(x => x.Id == id)
            .Select(x => x.UserName);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Dictionary<int, string?>> GetUserNamesByIds(IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        var query = _db.Users
            .AsNoTracking()
            .TagWith(nameof(UserRepository))
            .Where(x => !x.IsDisabled)
            .Where(x => ids.Contains(x.Id));
        return await query.ToDictionaryAsync(x => x.Id, x => x.UserName, cancellationToken);
    }

    public async Task DisableUser(int userId, CancellationToken cancellationToken = default)
    {
        var query = _db.Users
            .AsNoTracking()
            .TagWith(nameof(UserRepository))
            .Where(x => x.Id == userId);
        await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.IsDisabled, true), cancellationToken);
    }


    public async Task SetQuickLoginEnabled(int userId, bool enabled, CancellationToken cancellationToken = default)
    {
        var query = _db.Users
            .AsNoTracking()
            .TagWith(nameof(UserRepository))
            .Where(x => !x.IsDisabled)
            .Where(x => x.Id == userId);
        await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.QuickLogin, enabled), cancellationToken);
    }

    public async Task<bool> IsQuickLoginEnabledBySerial(string serial, CancellationToken cancellationToken = default)
    {
        var query = _db.Users
            .AsNoTracking()
            .TagWith(nameof(UserRepository))
            .Where(x => !x.IsDisabled)
            .Where(x => x.RegisterSerial == serial)
            .Select(x => x.QuickLogin);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> IsQuickLoginEnabledById(int id, CancellationToken cancellationToken = default)
    {
        var query = _db.Users
            .AsNoTracking()
            .TagWith(nameof(UserRepository))
            .Where(x => !x.IsDisabled)
            .Where(x => x.Id == id)
            .Select(x => x.QuickLogin);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserData?> GetUserByLogin(string login, CancellationToken cancellationToken = default)
    {
        var query = _userManager.Users
            .AsNoTracking()
            .TagWith(nameof(UserRepository))
            .IncludeAll()
            .Where(u => u.UserName == login);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserData?> GetUserByLoginCaseInsensitive(string login, CancellationToken cancellationToken = default)
    {
        var query = _userManager.Users
            .AsNoTracking()
            .TagWith(nameof(UserRepository))
            .IncludeAll()
            .Where(u => u.NormalizedUserName == login.ToUpper());
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> CountUsersBySerial(string serial, CancellationToken cancellationToken = default)
    {
        var query = _userManager.Users
            .TagWith(nameof(UserRepository))
            .Where(u => u.RegisterSerial == serial);
        return await query.CountAsync(cancellationToken);
    }

    public async Task<List<UserData>> GetUsersBySerial(string serial, CancellationToken cancellationToken = default)
    {
        var query = _userManager.Users
            .AsNoTracking()
            .TagWith(nameof(UserRepository))
            .IncludeAll()
            .Where(u => u.RegisterSerial == serial);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<UserData?> GetUserById(int id, CancellationToken cancellationToken = default)
    {
        var query = _userManager.Users
            .AsNoTracking()
            .TagWith(nameof(UserRepository))
            .IncludeAll()
            .Where(u => u.Id == id);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> IsUserNameInUse(string userName, CancellationToken cancellationToken = default)
    {
        var query = _userManager.Users
            .AsNoTracking()
            .Where(x => x.UserName == userName);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> IsUserNameInUseCaseInsensitive(string userName, CancellationToken cancellationToken = default)
    {
        var query = _userManager.Users
            .AsNoTracking()
            .TagWith(nameof(UserRepository))
            .Where(x => x.NormalizedUserName == userName.ToUpper());
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> TryUpdateLastNickName(int userId, string nick, CancellationToken cancellationToken = default)
    {
        var query = _db.Users
            .TagWith(nameof(UserRepository))
            .Where(x => x.Id == userId);
        var user = await query.FirstOrDefaultAsync();
        if (user == null)
            return false;

        user.Nick = nick;
        return await _db.SaveChangesAsync(cancellationToken) == 1;
        //return await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.Nick, nick), cancellationToken) == 1;
    }

    public async Task<string?> GetLastNickName(int userId, CancellationToken cancellationToken = default)
    {
        var query = _db.Users
            .AsNoTracking()
            .TagWith(nameof(UserRepository))
            .Where(x => x.Id == userId);
        var user = await query.FirstOrDefaultAsync(cancellationToken);
        return user?.Nick;
    }

    public async Task<int> CountBySerial(string serial, CancellationToken cancellationToken = default)
    {
        var query = _db.Users
            .AsNoTracking()
            .TagWith(nameof(UserRepository))
            .Where(x => x.RegisterSerial == serial);
        return await query.CountAsync(cancellationToken);
    }
}
