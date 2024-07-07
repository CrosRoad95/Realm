namespace RealmCore.Persistence.Repository;

public interface IUsersRepository
{
    Task<UserData?> GetBySerial(string serial, CancellationToken cancellationToken = default);
    Task<string?> GetLastNickName(int userId, CancellationToken cancellationToken = default);
    Task<string[]> GetRoles(int userId, CancellationToken cancellationToken = default);
}

internal sealed class UsersRepository : IUsersRepository
{
    private readonly IDb _db;

    public UsersRepository(IDb db)
    {
        _db = db;
    }

    public async Task<UserData?> GetBySerial(string serial, CancellationToken cancellationToken = default)
    {
        var query = _db.Users
            .AsNoTracking()
            .TagWithSource(nameof(UsersRepository))
            .Where(x => x.RegisterSerial == serial);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<string?> GetLastNickName(int userId, CancellationToken cancellationToken = default)
    {
        var query = _db.Users
            .AsNoTracking()
            .TagWithSource(nameof(UsersRepository))
            .Where(x => x.Id == userId);
        var user = await query.FirstOrDefaultAsync(cancellationToken);
        return user?.Nick;
    }

    public async Task<string[]> GetRoles(int userId, CancellationToken cancellationToken = default)
    {
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
}
