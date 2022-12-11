using Realm.Domain.Sessions;

namespace Realm.Server.Scripting.Events;

[NoDefaultScriptAccess]
public class PlayerSessionStoppedEvent : INamedLuaEvent, IDisposable
{
    private bool _disposed = false;
    private readonly RPGPlayer _rpgPlayer;
    private readonly SessionBase _session;

    public static string EventName => "onPlayerSessionStopped";

    [ScriptMember("player")]
    public RPGPlayer RPGPlayer
    {
        get
        {
            CheckIfDisposed();
            return _rpgPlayer;
        }
    }

    [ScriptMember("session", ScriptMemberFlags.ExposeRuntimeType)]
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

    public void Dispose()
    {
        _disposed = true;
    }
}
