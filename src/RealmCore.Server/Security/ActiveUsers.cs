namespace RealmCore.Server.Security;

internal sealed class ActiveUsers : IActiveUsers
{
    private readonly ConcurrentDictionary<int, Entity> _activeUsers = new();

    public IEnumerable<int> ActiveUsersIds => _activeUsers.Keys;
    public event Action<int, Entity>? Activated;
    public event Action<int, Entity>? Deactivated;

    public bool IsActive(int userId)
    {
        return _activeUsers.ContainsKey(userId);
    }

    public bool TrySetActive(int userId, Entity entity)
    {
        if(_activeUsers.TryAdd(userId, entity))
        {
            Activated?.Invoke(userId, entity);
            return true;
        }
        return false;
    }

    public bool TrySetInactive(int userId)
    {
        if (_activeUsers.TryRemove(userId, out var entity))
        {
            Deactivated?.Invoke(userId, entity);
            return true;
        }
        return false;
    }

    public bool TryGetEntityByUserId(int userId, out Entity? entity) => _activeUsers.TryGetValue(userId, out entity);
}
