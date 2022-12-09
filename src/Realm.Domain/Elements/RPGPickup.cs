namespace Realm.Domain.Elements;

[NoDefaultScriptAccess]
public class RPGPickup : Pickup, IDisposable, IWorldDebugData
{
    private bool _disposed;
    private readonly bool _isPersistant = PersistantScope.IsPersistant;

    private readonly Guid _debugId = Guid.NewGuid();
    [ScriptMember("debugId")]
    public Guid DebugId => _debugId;
    public PreviewType PreviewType => PreviewType.None;
    public Color PreviewColor => Color.FromArgb(100, 200, 0, 0);

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
