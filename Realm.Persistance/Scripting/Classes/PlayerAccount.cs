using Microsoft.ClearScript;

namespace Realm.Persistance.Scripting.Classes;

public class PlayerAccount
{
    private readonly User _user;
    private readonly UserManager<User> _userManager;

    [NoScriptAccess]
    public User User => _user;

    public string Id => _user.Id.ToString();
    public string UserName => _user.UserName;
    public PlayerAccount(User user, UserManager<User> userManager)
    {
        _user = user;
        _userManager = userManager;
    }

    public override string ToString() => _user.ToString();
}
