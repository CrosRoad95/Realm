namespace Realm.Server.Security;

internal class ActiveUsers : IActiveUsers
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly List<int> _activeUsersIds = new();

    public bool IsActive(int userId)
    {
        _lock.EnterReadLock();
        try
        {
            return _activeUsersIds.Contains(userId);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public bool TrySetActive(int userId)
    {

        _lock.EnterReadLock();
        try
        {
            if( _activeUsersIds.Contains(userId))
                return false;

            _activeUsersIds.Add(userId);
            return true;

        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public bool TrySetInactive(int userId)
    {

        _lock.EnterReadLock();
        try
        {
            if( !_activeUsersIds.Contains(userId))
                return false;

            _activeUsersIds.Remove(userId);
            return true;

        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
}
