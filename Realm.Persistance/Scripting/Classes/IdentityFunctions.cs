namespace Realm.Persistance.Scripting.Classes;

public class IdentityFunctions
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IDb _db;

    public IdentityFunctions(UserManager<User> userManager, RoleManager<Role> roleManager, IDb db)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
    }

    public async Task<PlayerAccount?> FindAccountById(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            return null;

        return new PlayerAccount(user, _userManager, _db);
    }
    
    public async Task<PlayerAccount?> FindAccountByUserName(string username)
    {
        var user = await _userManager.FindByNameAsync(username);

        if (user == null)
            return null;

        return new PlayerAccount(user, _userManager, _db);
    }
    
    public async Task<PlayerRole?> FindRoleByName(string name)
    {
        var role = await _roleManager.FindByNameAsync(name);

        if (role == null)
            return null;

        return new PlayerRole(role, _roleManager);
    }

    public async Task<List<PlayerRole>> GetAllRoles()
    {
        return await _roleManager.Roles.Select(x => new PlayerRole(x, _roleManager)).ToListAsync();
    }
    
    public async Task<List<PlayerAccount>> GetAllAccounts()
    {
        return await _userManager.Users.Select(x => new PlayerAccount(x, _userManager, _db)).ToListAsync();
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

    public async Task<PlayerRole> CreateRole(string name)
    {
        var result = await _roleManager.CreateAsync(new Role
        {
            Name = name,
        });

        if (!result.Succeeded)
        {
            throw new Exception(result.ToString());
        }

        var role = await FindRoleByName(name);
        if (role == null)
            throw new Exception("Failed to create an role");
        return role;
    }
}
