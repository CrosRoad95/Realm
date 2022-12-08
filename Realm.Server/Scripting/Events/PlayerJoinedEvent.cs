using Realm.Domain.Elements;

namespace Realm.Server.Scripting.Events;

public class PlayerJoinedEvent : INamedLuaEvent, IDisposable
{
    private bool _disposed = false;
    private readonly RPGPlayer _player;

    public static string EventName => "onPlayerJoin";

    public RPGPlayer Player
    {
        get
        {
            CheckIfDisposed();
            return _player;
        }
    }

    public PlayerJoinedEvent(RPGPlayer player)
    {
        _player = player;
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
