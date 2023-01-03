using SlipeServer.Server.Elements.ColShapes;

namespace Realm.Domain.Components.Elements;

public class CollisionSphereElementComponent : ElementComponent
{
    [Inject]
    private IRPGServer _rpgServer { get; set; } = default!;

    protected readonly CollisionSphere _collisionSphere;
    public Action<Entity>? EntityEntered { get; set; }
    public Action<Entity>? EntityLeft { get; set; }

    public override Element Element => _collisionSphere;

    public CollisionSphereElementComponent(CollisionSphere collisionSphere)
    {
        _collisionSphere = collisionSphere;
        _collisionSphere.ElementEntered += HandleElementEntered;
        _collisionSphere.ElementLeft += HandleElementLeft;
    }

    private void HandleElementEntered(Element element)
    {
        if (EntityEntered != null)
            EntityEntered(EntityByElement.GetByElement(element));
    }

    private void HandleElementLeft(Element element)
    {
        if (EntityLeft != null)
            EntityLeft(EntityByElement.GetByElement(element));
    }

    private void HandleDestroyed(Entity entity)
    {
        _collisionSphere.Destroy();
    }

    public override Task Load()
    {
        base.Load();
        Entity.Transform.Bind(_collisionSphere);
        _rpgServer.AssociateElement(new ElementHandle(_collisionSphere));
        Entity.Destroyed += HandleDestroyed;
        return Task.CompletedTask;
    }

}
