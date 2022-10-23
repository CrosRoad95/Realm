namespace Realm.Persistance.Scripting.Classes;

public class IdentityFunctions
{
    private readonly UserManager<User> _userManager;

    public IdentityFunctions(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<PlayerAccount?> FindAccountById(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            return null;

        return new PlayerAccount(user, _userManager);
    }
    
    public async Task<PlayerAccount?> FindAccountByUserName(string username)
    {
        var user = await _userManager.FindByNameAsync(username);

        if (user == null)
            return null;

        return new PlayerAccount(user, _userManager);
    }

    public async Task<PlayerAccount> CreateAccount(string username, string password)
    {
        var result = await _userManager.CreateAsync(new User
        {
            RegisteredDateTime = DateTime.Now,
            Nick = username,
            UserName = username,
        }, password);

        if(!result.Succeeded)
        {
            throw new Exception(result.ToString());
        }

        var account = await FindAccountByUserName(username);
        if (account == null)
            throw new Exception("Failed to create an account");
        return account;
    }
}
