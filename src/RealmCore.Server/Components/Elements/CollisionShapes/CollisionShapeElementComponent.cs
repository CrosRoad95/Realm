namespace RealmCore.Server.Components.Elements.CollisionShapes;

public abstract class CollisionShapeElementComponent : ElementComponent
{
    protected readonly CollisionShape _collisionShape;
    private readonly IEntityEngine _entityEngine;

    public Action<Entity, Entity>? EntityEntered { get; set; }
    public Action<Entity, Entity>? EntityLeft { get; set; }
    internal Action<CollisionShapeElementComponent>? CollidersRefreshed;

    internal override Element Element => _collisionShape;

    private readonly List<IEntityRule> _entityRules = new();
    private readonly object _entityRulesLock = new();

    protected CollisionShapeElementComponent(CollisionShape collisionShape, IEntityEngine entityEngine)
    {
        _collisionShape = collisionShape;
        _entityEngine = entityEngine;
        _collisionShape.ElementEntered += HandleElementEntered;
        _collisionShape.ElementLeft += HandleElementLeft;
    }

    public void RefreshColliders()
    {
        ThrowIfDisposed();

        CollidersRefreshed?.Invoke(this);
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
            if (!_entityEngine.TryGetByElement(element, out var entity))
                return;

            lock (_entityRulesLock)
                if (_entityRules.All(x => x.Check(entity)))
                    EntityEntered(Entity, entity);
        }
        catch (Exception ex)
        {
            // TODO: log
        }
    }

    private void HandleElementLeft(Element element)
    {
        ThrowIfDisposed();

        if (EntityLeft == null)
            return;

        try
        {
            if (!_entityEngine.TryGetByElement(element, out var entity))
                return;

            lock (_entityRulesLock)
                if (_entityRules.All(x => x.Check(entity)))
                    EntityLeft(Entity, entity);
        }
        catch (Exception ex)
        {
            // TODO: log
        }
    }

    public override void Dispose()
    {
        EntityEntered = null;
        EntityLeft = null;
        _collisionShape.ElementEntered -= HandleElementEntered;
        _collisionShape.ElementLeft -= HandleElementLeft;
        base.Dispose();
    }
}
