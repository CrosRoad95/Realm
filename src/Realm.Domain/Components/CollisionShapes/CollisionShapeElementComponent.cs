using Realm.Domain.Rules;

namespace Realm.Domain.Components.CollisionShapes;

public abstract class CollisionShapeElementComponent : ElementComponent
{
    [Inject]
    private ILogger<CollisionShapeElementComponent> Logger { get; set; } = default!;

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

    public void AddRule<TRule>() where TRule: IEntityRule, new()
    {
        ThrowIfDisposed();

        lock(_entityRulesLock)
            _entityRules.Add(new TRule());
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
            if(!EntityByElement.TryGetByElement(element, out var entity))
                throw new NullReferenceException(nameof(entity));

            lock(_entityRulesLock)
                if (_entityRules.All(x => x.Check(entity)))
                    EntityEntered(entity);
        }
        catch(Exception ex)
        {
            Logger.LogError(ex, "Failed to handle element entered.");
        }
    }

    private void HandleElementLeft(Element element)
    {
        ThrowIfDisposed();

        if (EntityLeft == null)
            return;

        try
        {
            if(!EntityByElement.TryGetByElement(element, out var entity))
                throw new NullReferenceException(nameof(entity));

            lock (_entityRulesLock)
                if (_entityRules.All(x => x.Check(entity)))
                    EntityLeft(entity);

            _collisionShape.ElementEntered -= HandleElementEntered;
            _collisionShape.ElementLeft -= HandleElementLeft;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to handle element left.");
        }
    }

    public override void Dispose()
    {
        _collisionShape.ElementEntered -= HandleElementEntered;
        _collisionShape.ElementLeft -= HandleElementLeft;
        base.Dispose();
    }
}
