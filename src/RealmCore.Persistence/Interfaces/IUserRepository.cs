namespace RealmCore.Persistence.Interfaces;

public interface IUserRepository : IRepositoryBase
{
    Task<int> CountUsersBySerial(string serial);
    Task DisableUser(int userId);
    Task<UserData?> GetUserById(int id);
    Task<UserData?> GetUserByLogin(string login);
    Task<UserData?> GetUserByLoginCaseInsensitive(string login);
    Task<UserData?> GetUserBySerial(string serial);
    Task<int> GetUserIdBySerial(string serial);
    Task<string?> GetUserNameById(int id);
    Task<Dictionary<int, string?>> GetUserNamesByIds(IEnumerable<int> ids);
    Task<List<UserData>> GetUsersBySerial(string serial);
    Task<bool> IsQuickLoginEnabledById(int id);
    Task<bool> IsQuickLoginEnabledBySerial(string serial);
    Task<bool> IsUserNameInUse(string userName);
    Task<bool> IsUserNameInUseCaseInsensitive(string userName);
    Task SetQuickLoginEnabled(int userId, bool enabled);
    Task<bool> TryUpdateLastNickName(int userId, string nick);
}
