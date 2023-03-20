namespace Realm.Server.Interfaces;

public interface IAccountService
{
    Task<bool> AuthorizePolicy(AccountComponent accountComponent, string policy);
}
