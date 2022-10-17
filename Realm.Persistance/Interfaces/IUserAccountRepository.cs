namespace Realm.Persistance.Interfaces;

public interface IUserAccountRepository
{
    Task<UserAccount> Create(string login, string password);
}
