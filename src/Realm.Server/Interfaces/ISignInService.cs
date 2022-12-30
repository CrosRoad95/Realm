using Realm.Persistance.Data;

namespace Realm.Server.Interfaces;

public interface ISignInService
{
    Task<bool> SignIn(Entity entity, User user);
}
