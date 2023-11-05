
namespace RealmCore.Server.Interfaces;

public interface IScopedElementFactory : IElementFactory, IDisposable
{
    IEnumerable<Element> CreatedElements { get; }
    Element? LastCreatedElement { get; }
    IEnumerable<CollisionShape> CollisionShapes { get; }

    IScopedElementFactory CreateScope();
    T GetLastCreatedElement<T>() where T : Element;
}
