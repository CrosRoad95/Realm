namespace RealmCore.Server.Concepts.Objectives;

public abstract class Objective : IDisposable
{
    private bool _disposed = false;
    private bool _isFulfilled = false;
    private Entity? _entity;
    private IEntityFactory? _entityFactory;

    public bool IsFulfilled => _isFulfilled;

    public event Action<Objective, object?>? Completed;
    public event Action<Objective>? Incompleted;
    public event Action<Objective>? Disposed;

    public Entity Entity { get => _entity ?? throw new InvalidOperationException(); internal set => _entity = value; }

    private PlayerPrivateElementComponent<BlipElementComponent>? _blipElementComponent;
    public abstract Vector3 Position { get; }
    protected ILogger Logger { get; set; } = default!;
    protected abstract void Load(IEntityFactory entityFactory, Entity playerEntity);
    public virtual void Update() { }

    internal void LoadInternal(IEntityFactory entityFactory, Entity playerEntity, ILogger logger)
    {
        ThrowIfDisposed();
        Logger = logger;
        _entityFactory = entityFactory;
        Load(entityFactory, playerEntity);
    }

    protected void Complete(Objective objective, object? data = null)
    {
        ThrowIfDisposed();

        if (Entity == null)
            throw new ArgumentNullException(nameof(Entity), "Entity cannot be null.");

        if (_isFulfilled)
            throw new ObjectiveAlreadyFulfilledException();

        if (_blipElementComponent != null)
            RemoveBlip();

        Completed?.Invoke(objective, data);
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

    public void AddBlip(BlipIcon blipIcon)
    {
        ThrowIfDisposed();

        if (_blipElementComponent != null)
            throw new InvalidOperationException();

        using var scopedEntityFactory = _entityFactory.CreateScopedEntityFactory(Entity);
        scopedEntityFactory.CreateBlip(blipIcon, Position);
        _blipElementComponent = scopedEntityFactory.GetLastCreatedComponent<PlayerPrivateElementComponent<BlipElementComponent>>();
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

        _disposed = true;

        Disposed?.Invoke(this);
    }
}
