namespace Realm.Server.Scripting;
    
public class PlayerFunctions
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly EventFunctions _eventFunctions;

    public PlayerFunctions(SignInManager<User> signInManager, UserManager<User> userManager, EventFunctions eventFunctions)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _eventFunctions = eventFunctions;
    }

    public async Task<bool> LogIn(RPGPlayer rpgPlayer, User user, string password)
    {
        if (rpgPlayer.ClaimsPrincipal != null)
            return false;

        if(await _userManager.CheckPasswordAsync(user, password))
        {
            var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(user);
            rpgPlayer.ClaimsPrincipal = claimsPrincipal;
            await _eventFunctions.InvokeEvent("onPlayerLogin", new PlayerLoggedInEvent
            {
                Player = rpgPlayer,
                Account = user,
            });
            return true;
        }
        return false;
    }

    public async Task<bool> LogOut(RPGPlayer rpgPlayer)
    {
        if (rpgPlayer.ClaimsPrincipal == null)
            return false;

        rpgPlayer.ClaimsPrincipal = null;
        await _eventFunctions.InvokeEvent("onPlayerLogout", new PlayerLoggedOutEvent
        {
            Player = rpgPlayer
        });
        return true;
    }
}

