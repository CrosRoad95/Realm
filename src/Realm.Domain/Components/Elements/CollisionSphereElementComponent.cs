namespace Realm.Domain.Components.Elements;

public class CollisionSphereElementComponent : ElementComponent
{
    protected readonly CollisionSphere _collisionSphere;

    public Action<Entity>? EntityEntered { get; set; }
    public Action<Entity>? EntityLeft { get; set; }

    internal override Element Element => _collisionSphere;

    internal CollisionSphereElementComponent(CollisionSphere collisionSphere, Entity? createForEntity = null) : base(createForEntity)
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
}
