namespace Realm.Domain.Interfaces;

public interface IEntityComponentsTransaction : IDisposable
{
    internal byte Version { get; }
    internal Entity Entity { get; }

    bool TryClose();
}
