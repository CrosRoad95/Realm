namespace RealmCore.Server.Modules.World.Triggers;

public interface ICollisionDetection
{
    event Action<Element>? ElementEntered;
    event Action<Element>? ElementLeft;
    CollisionDetection InternalCollisionDetection { get; }
    void CheckElementWithin(Element element);
}
