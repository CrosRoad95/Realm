namespace Realm.Domain;

internal class ElementHandle : IElementHandle
{
    private readonly Element _element;

    public ElementHandle(Element element)
    {
        _element = element;
    }

    public object GetElement() => _element;
}
