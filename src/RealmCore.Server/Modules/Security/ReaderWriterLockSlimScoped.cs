namespace RealmCore.Server.Modules.Security;

public sealed class ReaderWriterLockSlimScoped
{
    private readonly ReaderWriterLockSlim _lock;

    private struct ReadLockScope : IDisposable
    {
        public ReaderWriterLockSlim _lock;

        public void Dispose()
        {
            _lock.ExitReadLock();
        }
    }
    
    private struct WriteLockScope : IDisposable
    {
        public ReaderWriterLockSlim _lock;

        public void Dispose()
        {
            _lock.ExitWriteLock();
        }
    }

    public ReaderWriterLockSlimScoped(LockRecursionPolicy lockRecursionPolicy = LockRecursionPolicy.NoRecursion)
    {
        _lock = new(lockRecursionPolicy);
    }

    public IDisposable BeginRead()
    {
        _lock.EnterReadLock();

        return new ReadLockScope
        {
            _lock = _lock,
        };
    }

    public IDisposable BeginWrite()
    {
        _lock.EnterWriteLock();

        return new WriteLockScope
        {
            _lock = _lock,
        };
    }
}
