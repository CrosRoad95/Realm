namespace Realm.Server.Security;

internal class ActiveUsers : IActiveUsers
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly List<int> _usedAccountsIds = new();

    public bool IsActive(int accountId)
    {
        _lock.EnterReadLock();
        try
        {
            return _usedAccountsIds.Contains(accountId);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public bool TrySetActive(int accountId)
    {

        _lock.EnterReadLock();
        try
        {
            if( _usedAccountsIds.Contains(accountId))
                return false;

            _usedAccountsIds.Add(accountId);
            return true;

        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public bool TrySetInactive(int accountId)
    {

        _lock.EnterReadLock();
        try
        {
            if( !_usedAccountsIds.Contains(accountId))
                return false;

            _usedAccountsIds.Remove(accountId);
            return true;

        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
}
