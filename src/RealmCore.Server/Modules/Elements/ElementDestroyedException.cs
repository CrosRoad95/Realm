namespace RealmCore.Server.Modules.Elements;

public class ElementDestroyedException : Exception
{
    public Element Element { get; }
    public ElementDestroyedException(Element element)
    {
        Element = element;
    }
}
