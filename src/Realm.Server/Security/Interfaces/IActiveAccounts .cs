namespace Realm.Server.Security.Interfaces;

public interface IActiveUsers
{
    bool IsActive(int userId);
    bool TrySetActive(int userId);
    bool TrySetInactive(int userId);
}
