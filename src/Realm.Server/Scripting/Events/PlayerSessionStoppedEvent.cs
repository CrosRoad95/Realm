using Realm.Domain.Sessions;

namespace Realm.Server.Scripting.Events;

public class PlayerSessionStoppedEvent : INamedLuaEvent, IDisposable
{
    private bool _disposed = false;
    private readonly RPGPlayer _rpgPlayer;
    private readonly SessionBase _session;

    public static string EventName => "onPlayerSessionStopped";

    public RPGPlayer rpgPlayer
    {
        get
        {
            CheckIfDisposed();
            return _rpgPlayer;
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

    public PlayerSessionStoppedEvent(RPGPlayer rpgPlayer, SessionBase session)
    {
        _rpgPlayer = rpgPlayer;
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
