using Realm.Domain.Rules;
using SlipeServer.Packets.Structs;

namespace Realm.Domain.Components.Elements;

public class MarkerElementComponent : ElementComponent
{
    protected readonly Marker _marker;
    protected readonly CollisionSphere _collisionShape;
    internal override Element Element => _marker;
    internal CollisionSphere CollisionShape => _collisionShape;
    public Action<Entity>? EntityEntered { get; set; }
    public Action<Entity>? EntityLeft { get; set; }
    public Action<Entity, IEntityRule>? EntityRuleFailed { get; set; }

    public Color Color { get => _marker.Color; set => _marker.Color = value; }

    private readonly List<IEntityRule> _entityRules = new();

    internal MarkerElementComponent(Marker marker)
    {
        _marker = marker;
        _collisionShape = new CollisionSphere(marker.Position, _marker.Size);
    }

    public void AddRule(IEntityRule entityRule)
    {
        _entityRules.Add(entityRule);
    }

    public void AddRule<TEntityRole>() where TEntityRole : IEntityRule, new()
    {
        _entityRules.Add(new TEntityRole());
    }

    private void HandleElementEntered(Element element)
    {
        if (EntityEntered != null)
        {
            if (EntityByElement.TryGetByElement(element, out Entity entity))
                if (entity.Tag == Entity.EntityTag.Player || entity.Tag == Entity.EntityTag.Vehicle)
                {
                    foreach (var rule in _entityRules)
                    {
                        if (!rule.Check(entity))
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
        if (EntityLeft != null)
        {
            if(EntityByElement.TryGetByElement(element, out Entity entity))
                if (entity != null && (entity.Tag == Entity.EntityTag.Player || entity.Tag == Entity.EntityTag.Vehicle))
                    if (_entityRules.All(x => x.Check(entity)))
                        EntityLeft(entity);
        }
    }

    private void HandleDestroyed(Entity entity)
    {
        _collisionShape.ElementEntered -= HandleElementEntered;
        _collisionShape.ElementLeft -= HandleElementLeft;
        _collisionShape.Destroy();
    }

    protected override void Load()
    {
        base.Load();
        if (Entity.TryGetComponent(out PlayerElementComponent playerElementComponent))
            _collisionShape.Id = (ElementId)playerElementComponent.MapIdGenerator.GetId();

        Entity.Destroyed += HandleDestroyed;
        _collisionShape.ElementEntered += HandleElementEntered;
        _collisionShape.ElementLeft += HandleElementLeft;
        _collisionShape.Position = _marker.Position;
        if(!IsPerPlayer)
            Entity.Transform.PositionChanged += HandlePositionChanged;
    }

    private void HandlePositionChanged(Transform transform)
    {
        _collisionShape.Position = transform.Position;
    }

    public override void Dispose()
    {
        if(!IsPerPlayer)
            Entity.Transform.PositionChanged -= HandlePositionChanged;
        base.Dispose();
    }
}
