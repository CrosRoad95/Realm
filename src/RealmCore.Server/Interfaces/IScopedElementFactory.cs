

namespace RealmCore.Server.Interfaces;

public interface IScopedElementFactory : IElementFactory, IDisposable
{
    IEnumerable<Element> CreatedElements { get; }
    internal IEnumerable<ICollisionDetection> CreatedCollisionDetectionElements { get; }

    IScopedElementFactory CreateScope();
}
