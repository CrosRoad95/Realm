﻿namespace Realm.Domain.Elements;

[NoDefaultScriptAccess]
public class RPGRadarArea : RadarArea, IDisposable
{
    private bool _disposed;
    private readonly bool _isPersistant = PersistantScope.IsPersistant;
    public bool IsVariant { get; private set; }
    public RPGRadarArea() : base(Vector2.Zero, Vector2.Zero, Color.Transparent)
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