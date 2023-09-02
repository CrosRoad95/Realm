namespace RealmCore.Server.Interfaces;

public interface IUsersService
{
    Task<bool> AuthorizePolicy(UserComponent userComponent, string policy);
    void Kick(Entity entity, string reason);
    IEnumerable<Entity> SearchPlayersByName(string pattern);
    Task<bool> SignIn(Entity entity, UserData user);
    Task<int> SignUp(string username, string password);
    bool TryGetPlayerByName(string name, out Entity playerEntity);
    Task<bool> QuickSignIn(Entity entity);
    Task<bool> TryUpdateLastNickName(Entity playerEntity);
}
