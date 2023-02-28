namespace Realm.Server.Interfaces;

public interface IRPGUserManager
{
    Task<bool> SignIn(Entity entity, User user);
    Task<User?> SignUp(string username, string password);
}
