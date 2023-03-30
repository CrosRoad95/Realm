namespace Realm.Server.Security.Interfaces;

public interface IActiveUsers
{
    bool IsActive(int accountId);
    bool TrySetActive(int accountId);
    bool TrySetInactive(int accountId);
}
