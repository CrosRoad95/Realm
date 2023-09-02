namespace RealmCore.Persistence.Repository;

internal class UserRepository : IUserRepository
{
    private readonly IDb _db;
    private readonly UserManager<UserData> _userManager;

    public UserRepository(IDb db, UserManager<UserData> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public Task<UserData?> GetUserBySerial(string serial)
        => _db.Users
            .AsNoTrackingWithIdentityResolution()
            .Where(x => !x.IsDisabled)
            .Where(x => x.RegisterSerial == serial)
            .FirstOrDefaultAsync();
    
    public Task<string?> GetUserNameById(int id)
        => _db.Users
            .AsNoTrackingWithIdentityResolution()
            .Where(x => !x.IsDisabled)
            .Where(x => x.Id == id)
            .Select(x => x.UserName)
            .FirstOrDefaultAsync();
    
    public Task<int> GetUserIdBySerial(string serial)
        => _db.Users
            .AsNoTrackingWithIdentityResolution()
            .Where(x => !x.IsDisabled)
            .Where(x => x.RegisterSerial == serial)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

    public Task<string?> GetUserBySerial(int id)
        => _db.Users
            .AsNoTrackingWithIdentityResolution()
            .Where(x => !x.IsDisabled)
            .Where(x => x.Id == id)
            .Select(x => x.UserName)
            .FirstOrDefaultAsync();

    public Task<Dictionary<int, string?>> GetUserNamesByIds(IEnumerable<int> ids)
        => _db.Users
            .AsNoTrackingWithIdentityResolution()
            .Where(x => !x.IsDisabled)
            .Where(x => ids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.UserName);

    public Task DisableUser(int userId)
        => _db.Users.Where(x => x.Id == userId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsDisabled, true));
    
    public Task SetQuickLoginEnabled(int userId, bool enabled)
        => _db.Users
            .Where(x => !x.IsDisabled)
            .Where(x => x.Id == userId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.QuickLogin, enabled));
        
    public Task<bool> IsQuickLoginEnabledBySerial(string serial)
        => _db.Users
            .Where(x => !x.IsDisabled)
            .Where(x => x.RegisterSerial == serial)
            .Select(x => x.QuickLogin)
            .FirstOrDefaultAsync();
    
    public Task<bool> IsQuickLoginEnabledById(int id)
        => _db.Users
            .Where(x => !x.IsDisabled)
            .Where(x => x.Id == id)
            .Select(x => x.QuickLogin)
            .FirstOrDefaultAsync();

    public Task<UserData?> GetUserByLogin(string login)
    {
        return _userManager.Users
            .TagWith(nameof(UserRepository))
            .IncludeAll()
            .Where(u => u.UserName == login)
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync();
    }

    public Task<UserData?> GetUserByLoginCaseInsensitive(string login)
    {
        return _userManager.Users
            .TagWith(nameof(UserRepository))
            .IncludeAll()
            .Where(u => u.NormalizedUserName == login.ToUpper())
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync();
    }

    public Task<int> CountUsersBySerial(string serial)
    {
        return _userManager.Users
            .TagWith(nameof(UserRepository))
            .Where(u => u.RegisterSerial == serial)
            .AsNoTrackingWithIdentityResolution()
            .CountAsync();
    }

    public Task<List<UserData>> GetUsersBySerial(string serial)
    {
        return _userManager.Users
            .TagWith(nameof(UserRepository))
            .IncludeAll()
            .Where(u => u.RegisterSerial == serial)
            .AsNoTrackingWithIdentityResolution()
            .ToListAsync();
    }

    public Task<UserData?> GetUserById(int id)
    {
        return _userManager.Users
            .TagWith(nameof(UserRepository))
            .IncludeAll()
            .Where(u => u.Id == id)
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync();
    }

    public Task<bool> IsUserNameInUse(string userName)
    {
        return _userManager.Users.AnyAsync(u => u.UserName == userName);
    }

    public Task<bool> IsUserNameInUseCaseInsensitive(string userName)
    {
        return _userManager.Users.AnyAsync(u => u.NormalizedUserName == userName.ToUpper());
    }

    public async Task<bool> TryUpdateLastNickName(int userId, string nick)
    {
        return await _db.Users.Where(x => x.Id == userId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.Nick, nick)) == 1;
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    public Task<int> Commit()
    {
        return _db.SaveChangesAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await Commit();
        Dispose();
    }
}
