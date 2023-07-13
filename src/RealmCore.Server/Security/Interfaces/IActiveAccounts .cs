namespace RealmCore.Server.Security.Interfaces;

public interface IActiveUsers
{
    event Action<int>? Activated;
    event Action<int>? Deactivated;

    bool IsActive(int userId);
    bool TrySetActive(int userId);
    bool TrySetInactive(int userId);
}
