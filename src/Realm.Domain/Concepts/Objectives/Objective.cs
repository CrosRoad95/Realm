namespace Realm.Domain.Concepts.Objectives;

public abstract class Objective : IDisposable
{
    private bool _disposed = false;
    private bool _isFulfilled = false;
    private Entity? _entity;

    public bool IsFulfilled => _isFulfilled;

    public event Action<Objective>? Completed;
    public event Action<Objective>? Incompleted;

    public Entity Entity { get => _entity ?? throw new InvalidOperationException(); internal set => _entity = value; }

    public abstract void Load(IEntityFactory entityFactory, Entity playerEntity);

    protected void Complete()
    {
        ThrowIfDisposed();

        if (Entity == null)
            throw new ArgumentNullException(nameof(Entity), "Entity cannot be null.");

        if (_isFulfilled)
            throw new ObjectiveAlreadyFulfilledException();

        Completed?.Invoke(this);
        _isFulfilled = true;
    }

    public void Incomplete()
    {
        ThrowIfDisposed();

        if (Entity == null)
            throw new ArgumentNullException(nameof(Entity), "Entity cannot be null.");

        if (_isFulfilled)
            throw new ObjectiveAlreadyFulfilledException();

        Incompleted?.Invoke(this);
        _isFulfilled = true;
    }

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
