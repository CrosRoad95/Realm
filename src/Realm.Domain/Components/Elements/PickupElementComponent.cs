namespace Realm.Domain.Components.Elements;

public class PickupElementComponent : ElementComponent
{
    [Inject]
    private IRPGServer rpgServer { get; set; } = default!;

    protected readonly Pickup _pickup;
    public Action<Entity>? EntityEntered { get; set; }
    public Action<Entity>? EntityLeft { get; set; }

    public override Element Element => _pickup;

    public PickupElementComponent(Pickup pickup)
    {
        _pickup = pickup;
        _pickup.RespawnTime = 500;
    }

    private void HandleElementEntered(Element element)
    {
        if(EntityEntered != null)
            EntityEntered(EntityByElement.GetByElement(element));
    }

    private void HandleElementLeft(Element element)
    {
        if(EntityLeft != null)
            EntityLeft(EntityByElement.GetByElement(element));
    }

    private void HandleDestroyed(Entity entity)
    {
        _pickup.CollisionShape.ElementEntered -= HandleElementEntered;
        _pickup.CollisionShape.ElementLeft -= HandleElementLeft;
        _pickup.CollisionShape.Destroy();
        _pickup.Destroy();
    }

    public override Task Load()
    {
        base.Load();
        Entity.Transform.Bind(_pickup);
        _pickup.CollisionShape.Position = _pickup.Position;
        rpgServer.AssociateElement(new ElementHandle(_pickup));
        rpgServer.AssociateElement(new ElementHandle(_pickup.CollisionShape));
        Entity.Destroyed += HandleDestroyed;
        _pickup.CollisionShape.ElementEntered += HandleElementEntered;
        _pickup.CollisionShape.ElementLeft += HandleElementLeft;
        return Task.CompletedTask;
    }

}
