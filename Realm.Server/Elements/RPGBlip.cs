namespace Realm.Server.Elements;

public class RPGBlip : Blip, IDisposable
{
    private bool _disposed;
    private readonly bool _isPersistant = PersistantScope.IsPersistant;
    public bool IsVariant { get; private set; }
    public RPGBlip() : base(Vector3.Zero, BlipIcon.Marker, 250, 0)
    {
    }

    [NoScriptAccess]
    public void SetIsVariant()
    {
        CheckIfDisposed();
        IsVariant = true;
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
