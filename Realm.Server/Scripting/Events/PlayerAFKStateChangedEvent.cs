﻿namespace Realm.Server.Scripting.Events;

[NoDefaultScriptAccess]
public class PlayerAFKStateChangedEvent : INamedLuaEvent, IDisposable
{
    private bool _disposed = false;
    private readonly RPGPlayer _player;
    private readonly bool _afk;

    public static string EventName => "onPlayerAFKStateChanged";

    [ScriptMember("player")]
    public RPGPlayer Player
    {
        get
        {
            CheckIfDisposed();
            return _player;
        }
    }

    [ScriptMember("isAfk")]
    public bool IsAFK
    {
        get
        {
            CheckIfDisposed();
            return _afk;
        }
    }

    public PlayerAFKStateChangedEvent(RPGPlayer player, bool afk)
    {
        _player = player;
        _afk = afk;
    }

    private void CheckIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }

    [NoScriptAccess]
    public void Dispose()
    {
        _disposed = true;
    }
}