namespace Realm.Server.Interfaces;

public interface IRPGUserManager
{
    Task<bool> AuthorizePolicy(AccountComponent accountComponent, string policy);
    Task<bool> CheckPasswordAsync(User user, string password);
    Task<User?> GetUserByLogin(string login);
    Task<bool> IsUserNameInUse(string userName);
    Task<bool> SignIn(Entity entity, User user);
    Task<int> SignUp(string username, string password);
}
