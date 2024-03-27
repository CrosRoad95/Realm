namespace RealmCore.Server.Modules.World.Triggers;

public interface ICollisionDetection
{
    internal event Action<Element>? ElementEntered;
    internal event Action<Element>? ElementLeft;
    CollisionDetection InternalCollisionDetection { get; }
    void CheckElementWithin(Element element, bool matchInterior = true, bool matchDimension = true);
}
