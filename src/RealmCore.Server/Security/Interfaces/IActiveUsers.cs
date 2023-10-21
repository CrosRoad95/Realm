
namespace RealmCore.Server.Security.Interfaces;

public interface IActiveUsers
{
    IEnumerable<int> ActiveUsersIds { get; }

    bool IsActive(int userId);
    bool TryGetEntityByUserId(int userId, out Entity? entity);
    bool TrySetActive(int userId, Entity entity);
    bool TrySetInactive(int userId);
}
