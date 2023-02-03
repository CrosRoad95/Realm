using Realm.Domain.Rules;

namespace Realm.Domain.Components.CollisionShapes;

public abstract class CollisionShapeElementComponent : ElementComponent
{
    [Inject]
    private ILogger Logger { get; set; } = default!;

    protected readonly CollisionShape _collisionShape;

    public Action<Entity>? EntityEntered { get; set; }
    public Action<Entity>? EntityLeft { get; set; }

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

    public void AddRule(IEntityRule entityRule)
    {
        ThrowIfDisposed();

        lock(_entityRulesLock)
            _entityRules.Add(entityRule);
    }

    private void HandleElementEntered(Element element)
    {
        ThrowIfDisposed();

        if (EntityEntered == null)
            return;

        try
        {
            var entity = EntityByElement.TryGetByElement(element);
            if(entity == null)
                throw new NullReferenceException(nameof(entity));

            lock(_entityRulesLock)
                if (_entityRules.All(x => x.Check(entity)))
                    EntityEntered(entity);
        }
        catch(Exception ex)
        {
            Logger.Error(ex, "Failed to handle element entered.");
        }
    }

    private void HandleElementLeft(Element element)
    {
        ThrowIfDisposed();

        if (EntityLeft == null)
            return;

        try
        {
            var entity = EntityByElement.TryGetByElement(element);
            if (entity == null)
                throw new NullReferenceException(nameof(entity));

            lock (_entityRulesLock)
                if (_entityRules.All(x => x.Check(entity)))
                    EntityLeft(entity);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to handle element left.");
        }
    }
}
