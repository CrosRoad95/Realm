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

public sealed class ReaderWriterLockSlimScopedAsync
{
    private readonly SemaphoreSlim _lock;

    private struct ReadLockScope : IDisposable
    {
        public SemaphoreSlim _lock;

        public void Dispose()
        {
            _lock.Release();
        }
    }
    
    private struct WriteLockScope : IDisposable
    {
        public SemaphoreSlim _lock;

        public void Dispose()
        {
            _lock.Release();
        }
    }

    public ReaderWriterLockSlimScopedAsync()
    {
        _lock = new(1);
    }

    public IDisposable Begin(CancellationToken cancellationToken = default)
    {
        if(!_lock.Wait(1000, cancellationToken))
            throw new TimeoutException();

        return new ReadLockScope
        {
            _lock = _lock,
        };
    }

    public async Task<IDisposable> BeginAsync(CancellationToken cancellationToken = default)
    {
        if (!await _lock.WaitAsync(1000, cancellationToken))
            throw new TimeoutException();

        return new ReadLockScope
        {
            _lock = _lock,
        };
    }
}
