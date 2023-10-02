namespace RealmCore.Persistence.Interfaces;

public interface IUserRepository
{
    Task<int> CountUsersBySerial(string serial, CancellationToken cancellationToken = default);
    Task DisableUser(int userId, CancellationToken cancellationToken = default);
    Task<string?> GetLastNickName(int userId, CancellationToken cancellationToken = default);
    Task<UserData?> GetUserById(int id, CancellationToken cancellationToken = default);
    Task<UserData?> GetUserByLogin(string login, CancellationToken cancellationToken = default);
    Task<UserData?> GetUserByLoginCaseInsensitive(string login, CancellationToken cancellationToken = default);
    Task<UserData?> GetUserBySerial(string serial, CancellationToken cancellationToken = default);
    Task<int> GetUserIdBySerial(string serial, CancellationToken cancellationToken = default);
    Task<string?> GetUserNameById(int id, CancellationToken cancellationToken = default);
    Task<Dictionary<int, string?>> GetUserNamesByIds(IEnumerable<int> ids, CancellationToken cancellationToken = default);
    Task<List<UserData>> GetUsersBySerial(string serial, CancellationToken cancellationToken = default);
    Task<bool> IsQuickLoginEnabledById(int id, CancellationToken cancellationToken = default);
    Task<bool> IsQuickLoginEnabledBySerial(string serial, CancellationToken cancellationToken = default);
    Task<bool> IsUserNameInUse(string userName, CancellationToken cancellationToken = default);
    Task<bool> IsUserNameInUseCaseInsensitive(string userName, CancellationToken cancellationToken = default);
    Task SetQuickLoginEnabled(int userId, bool enabled, CancellationToken cancellationToken = default);
    Task<bool> TryUpdateLastNickName(int userId, string nick, CancellationToken cancellationToken = default);
}
