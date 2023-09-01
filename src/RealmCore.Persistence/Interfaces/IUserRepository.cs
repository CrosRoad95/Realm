namespace RealmCore.Persistence.Interfaces;

public interface IUserRepository : IRepositoryBase
{
    Task DisableUser(int userId);
    Task<UserData?> GetUserBySerial(string serial);
    Task<int> GetUserIdBySerial(string serial);
    Task<string?> GetUserNameById(int id);
    Task<Dictionary<int, string?>> GetUserNamesByIds(IEnumerable<int> ids);
    Task<bool> IsQuickLoginEnabledById(int id);
    Task<bool> IsQuickLoginEnabledBySerial(string serial);
    Task SetQuickLoginEnabled(int userId, bool enabled);
}
