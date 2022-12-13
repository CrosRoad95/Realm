using Realm.Resources.AdminTools.Data;

namespace Realm.Domain.Elements;

[NoDefaultScriptAccess]
public class RPGBlip : Blip, IDisposable, IWorldDebugData
{
    private bool _disposed;
    private readonly bool _isPersistant = PersistantScope.IsPersistant;
    public bool IsVariant { get; private set; }

    public DebugData DebugData => new(PreviewType.BoxWireframe, Color.FromArgb(100, 200, 200, 0));

    public RPGBlip() : base(Vector3.Zero, BlipIcon.Marker, 250, 0)
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
