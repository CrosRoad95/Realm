namespace RealmCore.Server.Concepts.Objectives;

public abstract class Objective : IDisposable
{
    private bool _isFulfilled = false;
    private Entity? _entity;
    private IEntityFactory? _entityFactory;

    public bool IsFulfilled => _isFulfilled;

    public event Action<Objective, object?>? Completed;
    public event Action<Objective>? InCompleted;
    public event Action<Objective>? Disposed;

    public Entity Entity { get => _entity ?? throw new InvalidOperationException(); internal set => _entity = value; }

    private PlayerPrivateElementComponent<BlipElementComponent>? _blipElementComponent;
    public abstract Vector3 Position { get; }
    protected ILogger Logger { get; set; } = default!;
    protected abstract void Load(IServiceProvider serviceProvider, Entity playerEntity);
    public virtual void Update() { }

    internal void LoadInternal(IServiceProvider serviceProvider, Entity playerEntity, ILogger logger)
    {
        Logger = logger;
        _entityFactory = serviceProvider.GetRequiredService<IEntityFactory>();
        Load(serviceProvider, playerEntity);
    }

    protected void Complete(Objective objective, object? data = null)
    {
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
        if (Entity == null)
            throw new ArgumentNullException(nameof(Entity), "Entity cannot be null.");

        if (_isFulfilled)
            throw new ObjectiveAlreadyFulfilledException();

        if (_blipElementComponent != null)
            RemoveBlip();

        InCompleted?.Invoke(objective);
        _isFulfilled = true;
    }

    public void AddBlip(BlipIcon blipIcon)
    {
        if (_blipElementComponent != null)
            throw new InvalidOperationException();

        using var scopedEntityFactory = _entityFactory.CreateScopedEntityFactory(Entity);
        scopedEntityFactory.CreateBlip(blipIcon, Position);
        _blipElementComponent = scopedEntityFactory.GetLastCreatedComponent<PlayerPrivateElementComponent<BlipElementComponent>>();
    }

    public void RemoveBlip()
    {
        if (_blipElementComponent == null)
            throw new InvalidOperationException();

        Entity.DestroyComponent(_blipElementComponent);
        _blipElementComponent = null;
    }

    public virtual void Dispose()
    {
        if (_blipElementComponent != null)
            RemoveBlip();

        Disposed?.Invoke(this);
    }
}
