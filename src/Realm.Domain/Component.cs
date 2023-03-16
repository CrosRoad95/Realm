namespace Realm.Domain;

public abstract class Component
{
    internal object _versionLock;
    internal byte _version;

    private bool _disposed = false;
    public Entity Entity { get; internal set; } = default!;

    public event Action<Component>? Disposed;
    protected virtual Task LoadAsync() => Task.CompletedTask;
    protected virtual void Load() { }

    internal void InternalLoad()
    {
        ThrowIfDisposed();
        Load();
    }

    internal async Task InternalLoadAsync()
    {
        ThrowIfDisposed();
        await LoadAsync();
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
