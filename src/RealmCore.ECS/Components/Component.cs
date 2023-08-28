using RealmCore.ECS.Interfaces;

namespace RealmCore.ECS.Components;

public abstract class Component : IComponent, IDisposable
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
            lock (_entityLock)
                return _entity ?? throw new InvalidOperationException();
        }
        internal set
        {
            lock (_entityLock)
                _entity = value;
        }
    }

    public event Action<Component>? Attached;
    public event Action<Component>? Detached;
    public event Action<Component>? Disposed;

    public virtual bool IsAsync() => false;

    protected virtual void Attach() { }
    protected virtual void Detach() { }

    internal void InternalDetach()
    {
        Detached?.Invoke(this);
        Detach();
    }

    internal void InternalAttach()
    {
        ThrowIfDisposed();
        Attached?.Invoke(this);
        Attach();
    }

    protected void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().Name);
    }

    internal bool TryRemoveEntity(Action callback)
    {
        lock (_entityLock)
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
        lock (_entityLock)
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
