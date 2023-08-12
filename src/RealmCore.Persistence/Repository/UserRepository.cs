namespace RealmCore.Persistence.Repository;

internal class UserRepository : IUserRepository
{
    private readonly IDb _db;

    public UserRepository(IDb db)
    {
        _db = db;
    }

    public Task<string?> GetUserNameById(int id)
        => _db.Users
            .Where(x => !x.IsDisabled)
            .Where(x => x.Id == id)
            .Select(x => x.UserName)
            .FirstOrDefaultAsync();

    public Task<Dictionary<int, string?>> GetUserNamesByIds(IEnumerable<int> ids)
        => _db.Users
            .Where(x => !x.IsDisabled)
            .Where(x => ids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.UserName);

    public Task DisableUser(int userId)
        => _db.Users.Where(x => x.Id == userId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsDisabled, true));

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
