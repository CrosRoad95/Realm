namespace RealmCore.Server.Components.Common;

public class OwnerComponent : ComponentLifecycle
{
    public Element OwningElement { get; }

    public OwnerComponent(Element owningElement)
    {
        OwningElement = owningElement;
        OwningElement.Destroyed += HandleDestroyed;
    }

    private void HandleDestroyed(Element element)
    {
        ((IComponents)Element).Components.TryDestroyComponent(this);
    }

    public override void Detach()
    {
        OwningElement.Destroyed -= HandleDestroyed;
    }
}
