using Realm.Domain.Sessions;

namespace Realm.Server.Scripting.Events;

[NoDefaultScriptAccess]
public class PlayerSessionStartedEvent : INamedLuaEvent, IDisposable
{
    private bool _disposed = false;
    private readonly Player _player;
    private readonly SessionBase _session;

    public static string EventName => "onPlayerSessionStarted";

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

    public PlayerSessionStartedEvent(Player player, SessionBase session)
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
