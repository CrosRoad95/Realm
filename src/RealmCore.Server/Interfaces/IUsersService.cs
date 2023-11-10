
namespace RealmCore.Server.Interfaces;

public interface IUsersService
{
    event Action<RealmPlayer>? SignedIn;
    event Action<RealmPlayer>? SignedOut;

    ValueTask<bool> AuthorizePolicy(RealmPlayer player, string policy, bool useCache = true);
    Task<bool> QuickSignIn(RealmPlayer player);
    IEnumerable<RealmPlayer> SearchPlayersByName(string pattern, bool loggedIn = true);
    Task<bool> SignIn(RealmPlayer player, UserData user);
    Task SignOut(RealmPlayer player);
    Task<int> SignUp(string username, string password);
    bool TryFindPlayerBySerial(string serial, out RealmPlayer? player);
    bool TryGetPlayerByName(string name, out RealmPlayer? player);
}
