namespace RealmCore.Persistence.Interfaces;

public interface IUserRepository : IRepositoryBase
{
    Task<string?> GetUserNameById(int id);
    Task<Dictionary<int, string?>> GetUserNamesByIds(IEnumerable<int> ids);
}
