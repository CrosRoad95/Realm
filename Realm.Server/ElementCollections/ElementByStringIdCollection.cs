namespace Realm.Server.ElementCollections;

public sealed class ElementByStringIdCollection
{
    private readonly Dictionary<string, Element> _elementsById = new();
    private readonly Dictionary<Element, string> _idByElement = new();

    public bool AssignElementToId(Element element, string id)
    {
        if(_elementsById.ContainsKey(id))
            return false;
        _elementsById[id] = element;
        _idByElement[element] = id;
        element.Destroyed += Element_Destroyed;
        return true;
    }

    private void Element_Destroyed(Element element)
    {
        var id = GetElementId(element);
        _elementsById.Remove(id);
        _idByElement.Remove(element);
        element.Destroyed -= Element_Destroyed;
    }

    public string? GetElementId(Element element)
    {
        if (_idByElement.TryGetValue(element, out string? id))
            return id;
        return null;
    }

    public Element? GetElementById(string id)
    {
        if (_elementsById.TryGetValue(id, out Element? element))
            return element;
        return null;
    }
}
