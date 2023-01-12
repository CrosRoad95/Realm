namespace Realm.Server.Interfaces;

public interface ISignInService
{
    Task<bool> SignIn(Entity entity, User user);
}
