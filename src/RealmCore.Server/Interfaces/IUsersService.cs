
namespace RealmCore.Server.Interfaces;

public interface IUsersService
{
    event Action<RealmPlayer>? SignedIn;
    event Action<RealmPlayer>? SignedOut;

    ValueTask<bool> AuthorizePolicy(RealmPlayer player, string policy, bool useCache = true);
    Task<bool> QuickSignIn(RealmPlayer player, CancellationToken cancellationToken = default);
    IEnumerable<RealmPlayer> SearchPlayersByName(string pattern, bool loggedIn = true);
    Task<bool> SignIn(RealmPlayer player, UserData user, CancellationToken cancellationToken = default);
    Task SignOut(RealmPlayer player, CancellationToken cancellationToken = default);
    Task<int> SignUp(string username, string password, CancellationToken cancellationToken = default);
    bool TryFindPlayerBySerial(string serial, out RealmPlayer? player);
    bool TryGetPlayerByName(string name, out RealmPlayer? player);
}
