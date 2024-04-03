namespace RealmCore.Server.Modules.Elements;

public interface IScopedElementFactory : IElementFactory, IDisposable
{
    IEnumerable<Element> CreatedElements { get; }
    RealmPlayer Player { get; }
    internal IEnumerable<Element> CreatedCollisionDetectionElements { get; }

    void AssociateWithPlayer(Element element);
    IScopedElementFactory CreateScope();
}
