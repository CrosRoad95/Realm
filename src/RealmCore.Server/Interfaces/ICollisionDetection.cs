namespace RealmCore.Server.Interfaces;

internal interface ICollisionDetection
{
    event Action<Element>? ElementEntered;
    event Action<Element>? ElementLeft;
    CollisionDetection InternalCollisionDetection { get; }
    void CheckElementWithin(Element element);
}
