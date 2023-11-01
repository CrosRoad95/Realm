namespace RealmCore.Server.Interfaces;

public interface IScopedElementFactory : IElementFactory, IDisposable
{
    IEnumerable<Element> CreatedElements { get; }
    Element? LastCreatedElement { get; }

    T GetLastCreatedElement<T>() where T : Element;
}
