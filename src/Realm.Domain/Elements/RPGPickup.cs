using Realm.Resources.AdminTools.Data;

namespace Realm.Domain.Elements;

[NoDefaultScriptAccess]
public class RPGPickup : Pickup, IDisposable, IWorldDebugData
{
    private bool _disposed;
    private readonly bool _isPersistant = PersistantScope.IsPersistant;

    public DebugData DebugData => new(PreviewType.None, Color.FromArgb(100, 200, 0, 0));
    public bool IsVariant { get; private set; }
    public RPGPickup() : base(Vector3.Zero, SlipeServer.Server.Elements.Enums.PickupModel.InfoIcon)
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
