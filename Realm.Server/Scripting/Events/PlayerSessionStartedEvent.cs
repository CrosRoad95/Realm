﻿using Realm.Domain.Elements;
using Realm.Domain.Sessions;

namespace Realm.Server.Scripting.Events;

public class PlayerSessionStartedEvent : INamedLuaEvent, IDisposable
{
    private bool _disposed = false;
    private readonly RPGPlayer _player;
    private readonly SessionBase _session;

    public static string EventName => "onPlayerSessionStarted";

    public RPGPlayer Player
    {
        get
        {
            CheckIfDisposed();
            return _player;
        }
    }
    
    public SessionBase Session
    {
        get
        {
            CheckIfDisposed();
            return _session;
        }
    }

    public PlayerSessionStartedEvent(RPGPlayer player, SessionBase session)
    {
        _player = player;
        _session = session;
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