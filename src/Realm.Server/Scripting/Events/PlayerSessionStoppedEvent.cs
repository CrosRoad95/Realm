using Realm.Domain.Sessions;

namespace Realm.Server.Scripting.Events;

[NoDefaultScriptAccess]
public class PlayerSessionStoppedEvent : INamedLuaEvent, IDisposable
{
    private bool _disposed = false;
    private readonly Player _player;
    private readonly SessionBase _session;

    public static string EventName => "onPlayerSessionStopped";

    [ScriptMember("player")]
    public Player Player
    {
        get
        {
            CheckIfDisposed();
            return _player;
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

    public PlayerSessionStoppedEvent(Player player, SessionBase session)
    {
        _player = player;
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
