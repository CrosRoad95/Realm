using Realm.Domain.Components;

namespace Realm.Domain.Concepts.Objectives;

public abstract class Objective : IDisposable
{
    private bool _disposed = false;
    private bool _isFulfilled = false;
    private Entity? _entity;

    public bool IsFulfilled => _isFulfilled;

    public event Action<Objective>? Completed;
    public event Action<Objective>? Incompleted;
    public event Action<Objective>? Disposed;

    public Entity Entity { get => _entity ?? throw new InvalidOperationException(); internal set => _entity = value; }

    private BlipElementComponent? _blipElementComponent;
    public abstract Vector3 Position { get; }
    protected ILogger Logger { get; set; } = default!;
    protected abstract void Load(IEntityFactory entityFactory, Entity playerEntity);

    internal void LoadInternal(IEntityFactory entityFactory, Entity playerEntity, ILogger logger)
    {
        ThrowIfDisposed();
        Logger = logger;
        Load(entityFactory, playerEntity);
    }

    protected void Complete(Objective objective)
    {
        ThrowIfDisposed();

        if (Entity == null)
            throw new ArgumentNullException(nameof(Entity), "Entity cannot be null.");

        if (_isFulfilled)
            throw new ObjectiveAlreadyFulfilledException();

        if (_blipElementComponent != null)
            RemoveBlip();

        Completed?.Invoke(objective);
        _isFulfilled = true;
    }

    public void Incomplete(Objective objective)
    {
        ThrowIfDisposed();

        if (Entity == null)
            throw new ArgumentNullException(nameof(Entity), "Entity cannot be null.");

        if (_isFulfilled)
            throw new ObjectiveAlreadyFulfilledException();

        if (_blipElementComponent != null)
            RemoveBlip();

        Incompleted?.Invoke(objective);
        _isFulfilled = true;
    }

    public void AddBlip(BlipIcon blipIcon, IEntityFactory entityFactory)
    {
        ThrowIfDisposed();

        if (_blipElementComponent != null)
            throw new InvalidOperationException();

        _blipElementComponent = entityFactory.CreateBlipFor(Entity, blipIcon, Position);
        _blipElementComponent.Disposed += HandleBlipElementComponentDisposed;
    }

    public void RemoveBlip()
    {
        ThrowIfDisposed();

        if (_blipElementComponent == null)
            throw new InvalidOperationException();

        Entity.DestroyComponent(_blipElementComponent);
        _blipElementComponent = null;
    }

    private void HandleBlipElementComponentDisposed(Component _)
    {
        _blipElementComponent = null;
    }

    protected void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(Objective));
    }

    public virtual void Dispose()
    {
        ThrowIfDisposed();
        if (_blipElementComponent != null)
            RemoveBlip();

        if (!_isFulfilled)
            Incomplete(this);

        _disposed = true;

        Disposed?.Invoke(this);
    }
}
