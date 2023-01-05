namespace Realm.Domain.Components.Elements;

public class PickupElementComponent : ElementComponent
{
    protected readonly Pickup _pickup;
    public Action<Entity>? EntityEntered { get; set; }
    public Action<Entity>? EntityLeft { get; set; }

    internal override Element Element => _pickup;

    public PickupElementComponent(Pickup pickup)
    {
        _pickup = pickup;
        _pickup.RespawnTime = 500;
    }

    private void HandleElementEntered(Element element)
    {
        if(EntityEntered != null)
        {
            var entity = EntityByElement.TryGetByElement(element);
            if(entity != null)
                EntityEntered(entity);
        }
    }

    private void HandleElementLeft(Element element)
    {
        if(EntityLeft != null)
        {
            var entity = EntityByElement.TryGetByElement(element);
            if (entity != null)
                EntityLeft(entity);
        }
    }

    private void HandleDestroyed(Entity entity)
    {
        _pickup.CollisionShape.ElementEntered -= HandleElementEntered;
        _pickup.CollisionShape.ElementLeft -= HandleElementLeft;
        _pickup.CollisionShape.Destroy();
    }

    public override Task Load()
    {
        Entity.Transform.Bind(_pickup);
        _pickup.CollisionShape.Position = _pickup.Position;
        _rpgServer.AssociateElement(new ElementHandle(_pickup));
        _rpgServer.AssociateElement(new ElementHandle(_pickup.CollisionShape));
        Entity.Destroyed += HandleDestroyed;
        _pickup.CollisionShape.ElementEntered += HandleElementEntered;
        _pickup.CollisionShape.ElementLeft += HandleElementLeft;
        return Task.CompletedTask;
    }

}
