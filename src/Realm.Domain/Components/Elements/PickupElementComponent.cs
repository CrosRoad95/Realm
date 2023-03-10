using Realm.Domain.Rules;

namespace Realm.Domain.Components.Elements;

public class PickupElementComponent : ElementComponent
{
    protected readonly Pickup _pickup;
    public Action<Entity>? EntityEntered { get; set; }
    public Action<Entity>? EntityLeft { get; set; }
    public Action<Entity, IEntityRule>? EntityRuleFailed { get; set; }

    internal override Element Element => _pickup;

    private readonly List<IEntityRule> _entityRules = new();

    internal PickupElementComponent(Pickup pickup)
    {
        _pickup = pickup;
        _pickup.RespawnTime = 500;
    }

    public void AddRule(IEntityRule entityRule)
    {
        _entityRules.Add(entityRule);
    }

    public void AddRule<TEntityRole>() where TEntityRole: IEntityRule, new()
    {
        _entityRules.Add(new TEntityRole());
    }

    private void HandleElementEntered(Element element)
    {
        if(EntityEntered != null)
        {
            if(EntityByElement.TryGetByElement(element, out var entity))
                if(entity.Tag == Entity.EntityTag.Player || entity.Tag == Entity.EntityTag.Vehicle)
                {
                    foreach (var rule in _entityRules)
                    {
                        if(!rule.Check(entity))
                        {
                            EntityRuleFailed?.Invoke(entity, rule);
                            return;
                        }
                    }
                    EntityEntered(entity);
                }
        }
    }

    private void HandleElementLeft(Element element)
    {
        if(EntityLeft != null)
        {
            if (EntityByElement.TryGetByElement(element, out var entity))
                if (entity.Tag == Entity.EntityTag.Player || entity.Tag == Entity.EntityTag.Vehicle)
                    if (_entityRules.All(x => x.Check(entity)))
                        EntityLeft(entity);
        }
    }

    private void HandleDestroyed(Entity entity)
    {
        _pickup.CollisionShape.ElementEntered -= HandleElementEntered;
        _pickup.CollisionShape.ElementLeft -= HandleElementLeft;
        _pickup.CollisionShape.Destroy();
    }

    protected override void Load()
    {
        base.Load();
        Entity.Destroyed += HandleDestroyed;
        _pickup.CollisionShape.ElementEntered += HandleElementEntered;
        _pickup.CollisionShape.ElementLeft += HandleElementLeft;
        _pickup.CollisionShape.Position = _pickup.Position;
        Entity.Transform.PositionChanged += HandlePositionChanged;
    }

    private void HandlePositionChanged(Transform transform)
    {
        _pickup.CollisionShape.Position = transform.Position;
    }

    public override void Dispose()
    {
        Entity.Transform.PositionChanged -= HandlePositionChanged;
    }
}
