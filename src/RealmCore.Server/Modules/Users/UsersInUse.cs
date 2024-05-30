namespace RealmCore.Server.Modules.Users;

public interface IUsersInUse
{
    IEnumerable<int> ActiveUsersIds { get; }

    bool IsActive(int userId);
    bool TryGetPlayerByUserId(int userId, out RealmPlayer player);
    bool TrySetActive(int userId, RealmPlayer player);
    bool TrySetInactive(int userId);
}

internal sealed class UsersInUse : IUsersInUse
{
    private readonly ConcurrentDictionary<int, RealmPlayer> _activeUsers = new();

    public IEnumerable<int> ActiveUsersIds => _activeUsers.Keys;
    public event Action<int, RealmPlayer>? Activated;
    public event Action<int, RealmPlayer>? Deactivated;

    public bool IsActive(int userId)
    {
        return _activeUsers.ContainsKey(userId);
    }

    public bool TrySetActive(int userId, RealmPlayer player)
    {
        if (userId < 0)
            throw new ArgumentOutOfRangeException(nameof(userId));

        if (_activeUsers.TryAdd(userId, player))
        {
            Activated?.Invoke(userId, player);
            return true;
        }
        return false;
    }

    public bool TrySetInactive(int userId)
    {
        if (userId < 0)
            throw new ArgumentOutOfRangeException(nameof(userId));

        if (_activeUsers.TryRemove(userId, out var player))
        {
            Deactivated?.Invoke(userId, player);
            return true;
        }
        return false;
    }

    public bool TryGetPlayerByUserId(int userId, out RealmPlayer player)
    {
        if (_activeUsers.TryGetValue(userId, out var tempPlayer) && tempPlayer != null)
        {
            player = tempPlayer;
            return true;
        }
        player = default!;
        return false;
    }
}
