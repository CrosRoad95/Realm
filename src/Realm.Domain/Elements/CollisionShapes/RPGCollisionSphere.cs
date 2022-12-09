using Realm.Module.Scripting.Scopes;
using SlipeServer.Server.Elements.ColShapes;

namespace Realm.Domain.Elements.CollisionShapes;

public class RPGCollisionSphere : CollisionSphere, IDisposable
{
    private bool _disposed;
    private readonly bool _isPersistant = PersistantScope.IsPersistant;

    public bool IsVariant { get; private set; }
    public RPGCollisionSphere() : base(Vector3.Zero, 0)
    {
    }

    public void SetIsVariant()
    {
        CheckIfDisposed();
        IsVariant = true;
    }

    [ScriptMember("isPersistant")]
    public bool IsPersistant()
    {
        CheckIfDisposed();
        return _isPersistant;
    }

    private void CheckIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }

    public void Dispose()
    {
        _disposed = true;
    }
}
