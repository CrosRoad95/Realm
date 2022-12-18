using Microsoft.EntityFrameworkCore;

namespace Realm.Domain.Persistance;

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
    public async Task<User?> FindAccountById(string id)
    {
        return await _userManager.FindByIdAsync(id);
    }

    [ScriptMember("findAccountByUserName")]
    public async Task<User?> FindAccountByUserName(string username)
    {
        return await _userManager.FindByNameAsync(username);
    }

    [ScriptMember("findRoleByName")]
    public async Task<Role?> FindRoleByName(string name)
    {
        return await _roleManager.FindByNameAsync(name);
    }

    [ScriptMember("getAllRoles")]
    public async Task<List<Role>> GetAllRoles()
    {
        return await _roleManager.Roles.ToListAsync();
    }

    [ScriptMember("getAllAccounts")]
    public async Task<List<User>> GetAllAccounts()
    {
        return await _userManager.Users.ToListAsync();
    }

    [ScriptMember("createAccount")]
    public async Task<User> CreateAccount(string username, string password)
    {
        var result = await _userManager.CreateAsync(new User
        {
            RegisteredDateTime = DateTime.Now,
            Nick = username,
            UserName = username,
        }, password);

        if (!result.Succeeded)
        {
            throw new Exception(result.ToString());
        }

        var account = await FindAccountByUserName(username);
        if (account == null)
            throw new Exception("Failed to create an account");
        return account;
    }

    [ScriptMember("createRole")]
    public async Task<Role> CreateRole(string name)
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
