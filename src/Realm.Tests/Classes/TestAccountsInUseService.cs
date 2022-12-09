using Realm.Interfaces.Server.Services;

namespace Realm.Tests.Classes;

internal class TestAccountsInUseService : IAccountsInUseService
{
    public bool IsAccountIdInUse(string id)
    {
        return false;
    }
}
