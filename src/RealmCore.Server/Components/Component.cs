namespace RealmCore.Server.Components;

public abstract class Component : IDisposable
{
    internal object _versionLock = new();
    internal object _entityLock = new();
    internal byte _version;

    protected bool _disposed = false;

    private Entity? _entity = null;
    public Entity Entity
    {
        get
        {
            lock(_entityLock)
                return _entity ?? throw new InvalidOperationException();
        }
        internal set
        {
            lock(_entityLock)
                _entity = value;
        }
    }

    public event Action<Component>? Disposed;
    public event Action<Component>? DetachedFromEntity;

    public virtual bool IsAsync() => false;

    protected virtual void Load() { }
    protected virtual void Detached() { }
    internal void InternalDetached() {
        DetachedFromEntity?.Invoke(this);
        Detached();
    }

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

    internal bool TryRemoveEntity(Action callback)
    {
        lock(_entityLock)
        {
            if (_entity == null)
                return false;

            callback();

            _entity = null;
            return true;
        }
    }
    
    internal bool TrySetEntity(Entity entity)
    {
        lock(_entityLock)
        {
            if (_entity != null)
                return false;

            _entity = entity;
            return true;
        }
    }

    public virtual void Dispose()
    {
        ThrowIfDisposed();
        Disposed?.Invoke(this);
        _disposed = true;
    }
}
