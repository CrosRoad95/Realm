using Realm.Domain.Rules;

namespace Realm.Domain.Components.Elements;

public abstract class CollisionShapeElementComponent : ElementComponent
{
    protected readonly CollisionShape _collisionShape;

    public Action<Entity>? EntityEntered { get; set; }
    public Action<Entity>? EntityLeft { get; set; }

    internal override Element Element => _collisionShape;

    private readonly List<IEntityRule> _entityRules = new();

    protected CollisionShapeElementComponent(CollisionShape collisionShape)
    {
        _collisionShape = collisionShape;
        _collisionShape.ElementEntered += HandleElementEntered;
        _collisionShape.ElementLeft += HandleElementLeft;
    }

    public void CheckCollisionWith(Entity entity)
    {
        if (entity.TryGetComponent(out ElementComponent elementComponent))
        {
            _collisionShape.CheckElementWithin(elementComponent.Element);
        }
    }

    public void AddRule(IEntityRule entityRule)
    {
        _entityRules.Add(entityRule);
    }

    private void HandleElementEntered(Element element)
    {
        if (EntityEntered != null)
        {
            var entity = EntityByElement.TryGetByElement(element);
            if (entity != null)
                if (_entityRules.All(x => x.Check(entity)))
                    EntityEntered(entity);
        }
    }

    private void HandleElementLeft(Element element)
    {
        if (EntityLeft != null)
        {
            var entity = EntityByElement.TryGetByElement(element);
            if (entity != null)
                if (_entityRules.All(x => x.Check(entity)))
                    EntityLeft(entity);
        }
    }
}
