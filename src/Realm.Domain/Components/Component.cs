namespace Realm.Domain.Components;

public abstract class Component : IDisposable
{
    internal object _versionLock;
    internal byte _version;

    private bool _disposed = false;
    public Entity Entity { get; internal set; } = default!;

    public event Action<Component>? Disposed;

    public virtual bool IsAsync() => false;

    protected virtual void Load() { }

    internal void InternalLoad()
    {
        ThrowIfDisposed();
        Load();
    }

    protected void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().Name);
    }

    public virtual void Dispose()
    {
        ThrowIfDisposed();
        _disposed = true;
        Disposed?.Invoke(this);
    }
}
