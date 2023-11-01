namespace RealmCore.Server.Components.Common;

public class OwnerDisposableComponent : Component
{
    public Element OwnerElement { get; }

    public OwnerDisposableComponent(Element ownerElement)
    {
        OwnerElement = ownerElement;
    }
}