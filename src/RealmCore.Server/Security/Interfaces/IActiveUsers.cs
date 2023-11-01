
namespace RealmCore.Server.Security.Interfaces;

public interface IActiveUsers
{
    IEnumerable<int> ActiveUsersIds { get; }

    bool IsActive(int userId);
    bool TryGetPlayerByUserId(int userId, out RealmPlayer? player);
    bool TrySetActive(int userId, RealmPlayer player);
    bool TrySetInactive(int userId);
}
