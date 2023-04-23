namespace RealmCore.Server.Components.Elements.CollisionShapes;

public abstract class CollisionShapeElementComponent : ElementComponent
{
    [Inject]
    private IECS ECS { get; set; } = default!;

    [Inject]
    private ILogger<CollisionShapeElementComponent> Logger { get; set; } = default!;

    protected readonly CollisionShape _collisionShape;

    public Action<Entity, Entity>? EntityEntered { get; set; }
    public Action<Entity, Entity>? EntityLeft { get; set; }

    internal override Element Element => _collisionShape;

    private readonly List<IEntityRule> _entityRules = new();
    private readonly object _entityRulesLock = new();

    protected CollisionShapeElementComponent(CollisionShape collisionShape)
    {
        _collisionShape = collisionShape;
        _collisionShape.ElementEntered += HandleElementEntered;
        _collisionShape.ElementLeft += HandleElementLeft;
    }

    public void CheckCollisionWith(Entity entity)
    {
        ThrowIfDisposed();

        if (entity.TryGetComponent(out ElementComponent elementComponent))
        {
            _collisionShape.CheckElementWithin(elementComponent.Element);
        }
    }

    public void AddRule<TRule>() where TRule : IEntityRule, new()
    {
        ThrowIfDisposed();

        lock (_entityRulesLock)
            _entityRules.Add(new TRule());
    }

    public void AddRule(IEntityRule entityRule)
    {
        ThrowIfDisposed();

        lock (_entityRulesLock)
            _entityRules.Add(entityRule);
    }

    private void HandleElementEntered(Element element)
    {
        ThrowIfDisposed();

        if (EntityEntered == null)
            return;

        try
        {
            if (!ECS.TryGetByElement(element, out var entity))
                return;

            lock (_entityRulesLock)
                if (_entityRules.All(x => x.Check(entity)))
                    EntityEntered(Entity, entity);
        }
        catch (Exception ex)
        {
            Logger.LogHandleError(ex);
        }
    }

    private void HandleElementLeft(Element element)
    {
        ThrowIfDisposed();

        if (EntityLeft == null)
            return;

        try
        {
            if (!ECS.TryGetByElement(element, out var entity))
                return;

            lock (_entityRulesLock)
                if (_entityRules.All(x => x.Check(entity)))
                    EntityLeft(Entity, entity);
        }
        catch (Exception ex)
        {
            Logger.LogHandleError(ex);
        }
    }

    public override void Dispose()
    {
        _collisionShape.ElementEntered -= HandleElementEntered;
        _collisionShape.ElementLeft -= HandleElementLeft;
        base.Dispose();
    }
}
