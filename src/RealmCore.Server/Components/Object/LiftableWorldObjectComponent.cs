using RealmCore.Server.Components.Abstractions;

namespace RealmCore.Server.Components.Object;

[ComponentUsage(false)]
public class LiftableWorldObjectComponent : InteractionComponent
{
    private readonly object _lock = new();
    public Element? Owner { get; private set; }
    public Element[]? AllowedForElements { get; private set; }

    public event Action<LiftableWorldObjectComponent, Element>? Lifted;
    public event Action<LiftableWorldObjectComponent, Element>? Dropped;

    public LiftableWorldObjectComponent() { }

    public LiftableWorldObjectComponent(params Element[] allowedForEntities)
    {
        AllowedForElements = allowedForEntities;
    }

    public bool TryLift(Element element)
    {
        if (!IsAllowedToLift(element))
            return false;

        lock (_lock)
        {
            if (Element == element)
                return false;

            if (Owner == null)
            {
                Owner = element;
                Owner.Destroyed += HandleOwnerDestroyed;
                Lifted?.Invoke(this, element);
                return true;
            }
        }
        return false;
    }

    public bool IsAllowedToLift(Element element) => AllowedForElements == null || AllowedForElements.Contains(element);

    private void HandleOwnerDestroyed(Element element)
    {
        Debug.Assert(Owner != null);
        Owner.Destroyed -= HandleOwnerDestroyed;
        TryDrop();
    }

    public bool TryDrop()
    {
        lock (_lock)
        {
            if (Owner == null)
                return false;

            Owner.Destroyed -= HandleOwnerDestroyed;
            Dropped?.Invoke(this, Owner);
            Owner = null;
        }
        return true;
    }
}
