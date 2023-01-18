using Realm.Domain.Concepts.Objectives;

namespace Realm.Domain;

public abstract class Component
{
    private bool _disposed = false;
    public Entity Entity { get; internal set; } = default!;

    public virtual Task Load() => Task.CompletedTask;

    protected void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(Objective));
    }

    public virtual void Dispose()
    {
        ThrowIfDisposed();
        _disposed = true;
    }
}
