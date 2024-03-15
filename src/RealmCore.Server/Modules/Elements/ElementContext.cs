namespace RealmCore.Server.Modules.Elements;

public sealed class ElementContext
{
    private Element? _element;

    public Element Element { get => _element ?? throw new InvalidElementContextException(); internal set => _element = value; }

    public void Set(Element element)
    {
        if (Element != null)
            throw new InvalidOperationException("Element already set");

        Element = element;
    }
}
