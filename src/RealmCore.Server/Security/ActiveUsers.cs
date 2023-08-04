namespace RealmCore.Server.Security;

internal class ActiveUsers : IActiveUsers
{
    private readonly object _lock = new();
    private readonly List<int> _activeUsersIds = new();

    public List<int> ActiveUsersIds
    {
        get
        {
            lock (_lock)
                return new List<int>(_activeUsersIds);
        }
    }

    public event Action<int>? Activated;
    public event Action<int>? Deactivated;

    public bool IsActive(int userId)
    {
        lock (_lock)
            return _activeUsersIds.Contains(userId);
    }

    public bool TrySetActive(int userId)
    {
        lock (_lock)
        {
            if (_activeUsersIds.Contains(userId))
                return false;

            _activeUsersIds.Add(userId);
            Activated?.Invoke(userId);
            return true;
        }
    }

    public bool TrySetInactive(int userId)
    {
        lock (_lock)
        {
            if (!_activeUsersIds.Contains(userId))
                return false;

            _activeUsersIds.Remove(userId);
            Deactivated?.Invoke(userId);
            return true;
        }
    }
}
