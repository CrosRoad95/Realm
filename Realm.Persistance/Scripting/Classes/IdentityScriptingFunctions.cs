namespace Realm.Persistance.Scripting.Classes;

[NoDefaultScriptAccess]
public class IdentityScriptingFunctions
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IServiceProvider _serviceProvider;

    public IdentityScriptingFunctions(UserManager<User> userManager, RoleManager<Role> roleManager, IServiceProvider serviceProvider)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _serviceProvider = serviceProvider;
    }

    [ScriptMember("findAccountById")]
    public async Task<PlayerAccount?> FindAccountById(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            return null;
        var playerAccount = _serviceProvider.GetRequiredService<PlayerAccount>();
        playerAccount.SetUser(user);
        return playerAccount;
    }

    [ScriptMember("findAccountByUserName")]
    public async Task<PlayerAccount?> FindAccountByUserName(string username)
    {
        var user = await _userManager.FindByNameAsync(username);

        if (user == null)
            return null;

        var playerAccount = _serviceProvider.GetRequiredService<PlayerAccount>();
        playerAccount.SetUser(user);
        return playerAccount;
    }

    [ScriptMember("findRoleByName")]
    public async Task<PlayerRole?> FindRoleByName(string name)
    {
        var role = await _roleManager.FindByNameAsync(name);

        if (role == null)
            return null;

        return new PlayerRole(role, _roleManager);
    }

    [ScriptMember("getAllRoles")]
    public async Task<List<PlayerRole>> GetAllRoles()
    {
        return await _roleManager.Roles.Select(x => new PlayerRole(x, _roleManager)).ToListAsync();
    }

    [ScriptMember("getAllAccounts")]
    public async Task<List<PlayerAccount>> GetAllAccounts()
    {
        return (await _userManager.Users.ToListAsync()).Select(user =>
        {
            var playerAccount = _serviceProvider.GetRequiredService<PlayerAccount>();
            playerAccount.SetUser(user);
            return playerAccount;
        }).ToList();
    }

    [ScriptMember("createAccount")]
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

    [ScriptMember("createRole")]
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
