namespace RealmCore.Server.Modules.World.Triggers;

public abstract class CollisionDetection
{
    internal abstract void RelayEntered(Element element);
    internal abstract void RelayLeft(Element element);
}

public class CollisionDetection<T> : CollisionDetection
{
    private readonly T _that;

    public event Action<T, Element>? Entered;
    public event Action<T, Element>? Left;

    public CollisionDetection(T that)
    {
        _that = that;
    }

    internal override void RelayEntered(Element element)
    {
        Entered?.Invoke(_that, element);
    }

    internal override void RelayLeft(Element element)
    {
        Left?.Invoke(_that, element);
    }
}
