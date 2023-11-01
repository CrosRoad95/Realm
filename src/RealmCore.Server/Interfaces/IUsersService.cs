
namespace RealmCore.Server.Interfaces;

public interface IUsersService
{
    event Action<RealmPlayer>? SignedIn;
    event Action<RealmPlayer>? SignedOut;

    Task<bool> AddUserEvent(RealmPlayer player, int eventId, string? metadata = null);
    ValueTask<bool> AuthorizePolicy(UserComponent userComponent, string policy, bool useCache = true);
    Task<List<UserEventData>> GetAllUserEvents(RealmPlayer player, IEnumerable<int>? events = null);
    Task<List<UserEventData>> GetLastUserEvents(RealmPlayer player, int limit = 10, IEnumerable<int>? events = null);
    Task<bool> QuickSignIn(RealmPlayer player);
    IEnumerable<RealmPlayer> SearchPlayersByName(string pattern);
    Task<bool> SignIn(RealmPlayer player, UserData user);
    Task SignOut(RealmPlayer player);
    Task<int> SignUp(string username, string password);
    bool TryFindPlayerBySerial(string serial, out RealmPlayer? player);
    bool TryGetPlayerByName(string name, out RealmPlayer? player);
    Task<bool> TryUpdateLastNickName(RealmPlayer player);
    Task<bool> UpdateLastNewsRead(RealmPlayer player);
}
