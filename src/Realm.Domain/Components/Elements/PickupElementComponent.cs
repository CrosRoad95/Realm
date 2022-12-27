using Realm.Domain.Interfaces;
using SlipeServer.Server.Elements;

namespace Realm.Domain.Components.Elements;

public class PickupElementComponent : ElementComponent
{
    protected readonly Pickup _pickup;
    public Action<Entity>? EntityEntered { get; set; }

    public override Element Element => _pickup;

    public PickupElementComponent(Pickup pickup)
    {
        _pickup = pickup;
    }

    private void HandleElementEntered(Element element)
    {
        if(EntityEntered != null)
            EntityEntered(ElementById.GetByElement(element));
    }

    private void HandleDestroyed(Entity entity)
    {
        _pickup.CollisionShape.ElementEntered -= HandleElementEntered;
        _pickup.CollisionShape.Destroy();
        _pickup.Destroy();
    }

    public override Task Load()
    {
        base.Load();
        Entity.Transform.Bind(_pickup);
        _pickup.CollisionShape.Position = _pickup.Position;
        var server = Entity.GetRequiredService<IRPGServer>();
        server.AssociateElement(new ElementHandle(_pickup));
        server.AssociateElement(new ElementHandle(_pickup.CollisionShape));
        Entity.Destroyed += HandleDestroyed;
        _pickup.CollisionShape.ElementEntered += HandleElementEntered;
        return Task.CompletedTask;
    }

}
