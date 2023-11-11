namespace RealmCore.Server.Interfaces;

public interface ICollisionDetection
{
    event Action<Element>? ElementEntered;
    event Action<Element>? ElementLeft;
    CollisionDetection InternalCollisionDetection { get; }
    void CheckElementWithin(Element element);
}
