﻿namespace RealmCore.Server.Modules.Elements.Interactions;

public class LiftableInteraction : Interaction
{
    private readonly object _lock = new();
    public Element? Owner { get; private set; }
    public Element[]? AllowedForElements { get; private set; }

    public event Action<LiftableInteraction, Element>? Lifted;
    public event Action<LiftableInteraction, Element>? Dropped;

    public LiftableInteraction() { }

    public LiftableInteraction(params Element[] allowedForElements)
    {
        AllowedForElements = allowedForElements;
    }

    public bool IsAllowedToLift(Element element) => AllowedForElements == null || AllowedForElements.Contains(element);

    private void HandleOwnerDestroyed(Element element)
    {
        Debug.Assert(Owner != null);
        Owner.Destroyed -= HandleOwnerDestroyed;
        TryDrop();
    }

    public bool TryLift(Element element)
    {
        if (!IsAllowedToLift(element))
            return false;

        lock (_lock)
        {
            if (Owner != null)
                return false;

            Owner = element;
            Owner.Destroyed += HandleOwnerDestroyed;
            Lifted?.Invoke(this, element);
        }
        return true;
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
