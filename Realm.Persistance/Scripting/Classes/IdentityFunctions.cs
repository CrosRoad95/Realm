namespace Realm.Persistance.Scripting.Classes;

public class IdentityFunctions
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;

    public IdentityFunctions(SignInManager<User> signInManager, UserManager<User> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public async Task<User?> FindAccountByUserName(string username)
    {
        return await _userManager.FindByNameAsync(username);
    }

    public async Task<User> CreateAccount(string username, string password)
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

        return await _userManager.FindByNameAsync(username);
    }
}
