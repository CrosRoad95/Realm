namespace RealmCore.Persistence.Repository;

internal class UserRepository : IUserRepository
{
    private readonly IDb _db;

    public UserRepository(IDb db)
    {
        _db = db;
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
