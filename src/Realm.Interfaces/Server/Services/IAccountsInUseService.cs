namespace Realm.Interfaces.Server.Services;

public interface IAccountsInUseService
{
    bool IsAccountIdInUse(string id);
}
