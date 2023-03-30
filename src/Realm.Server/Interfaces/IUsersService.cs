namespace Realm.Server.Interfaces;

public interface IUsersService
{
    Task<bool> AuthorizePolicy(AccountComponent accountComponent, string policy);
    Task<bool> CheckPasswordAsync(User user, string password);
    Task<User?> GetUserById(int id);
    Task<User?> GetUserByLogin(string login);
    Task<User?> GetUserByLoginCaseInsensitive(string login);
    Task<bool> IsSerialWhitelisted(int userId, string serial);
    Task<bool> IsUserNameInUse(string userName);
    Task<bool> SignIn(Entity entity, User user);
    Task<int> SignUp(string username, string password);
    Task<bool> TryAddWhitelistedSerial(int userId, string serial);
    Task<bool> TryRemoveWhitelistedSerial(int userId, string serial);
}
