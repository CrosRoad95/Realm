namespace RealmCore.Server.Components.Common;

public class OwnerComponent : Component
{
    public Element OwningElement { get; }

    public OwnerComponent(Element owningElement)
    {
        OwningElement = owningElement;
    }
}
