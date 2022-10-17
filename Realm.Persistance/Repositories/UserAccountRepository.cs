namespace Realm.Persistance.Repositories;

internal class UserAccountRepository : IUserAccountRepository
{
    private readonly IDb _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly DbSet<UserAccount> _set;

    public UserAccountRepository(IDb db, IPasswordHasher passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _set = _db.UserAccounts;
    }

    public async Task<UserAccount> Create(string login, string password)
    {
        var account = new UserAccount
        {
            Login = login,
            Password = _passwordHasher.HashPassword(password),
        };
        _set.Add(account);
        await _db.SaveChangesAsync();
        return account;
    }
}
