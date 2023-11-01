namespace RealmCore.Server.Interfaces;

public interface IScopedElementFactory : IElementFactory, IDisposable
{
    RealmPlayer Player { get; }
    IEnumerable<Element> CreatedElements { get; }
    Element? LastCreatedElement { get; }

    T GetLastCreatedElement<T>() where T : Element;
}
