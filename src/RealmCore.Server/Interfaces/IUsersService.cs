namespace RealmCore.Server.Interfaces;

public interface IUsersService
{
    Task<bool> AddUserEvent(Entity userEntity, int eventId, string? metadata = null);
    ValueTask<bool> AuthorizePolicy(UserComponent userComponent, string policy, bool useCache = true);
    Task<List<UserEventData>> GetAllUserEvents(Entity userEntity, IEnumerable<int>? events = null);
    Task<List<UserEventData>> GetLastUserEvents(Entity userEntity, int limit = 10, IEnumerable<int>? events = null);
    void Kick(Entity entity, string reason);
    Task<bool> QuickSignIn(Entity entity);
    IEnumerable<Entity> SearchPlayersByName(string pattern);
    Task<bool> SignIn(Entity entity, UserData user);
    Task<int> SignUp(string username, string password);
    bool TryGetPlayerByName(string name, out Entity? playerEntity);
    Task<bool> TryUpdateLastNickName(Entity playerEntity);
}
