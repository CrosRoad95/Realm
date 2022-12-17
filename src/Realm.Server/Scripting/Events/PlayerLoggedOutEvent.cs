namespace Realm.Server.Scripting.Events;

[NoDefaultScriptAccess]
public class PlayerLoggedOutEvent : INamedLuaEvent, IDisposable
{
    private bool _disposed = false;
    private readonly Player _player;

    public static string EventName => "onPlayerLogout";

    [ScriptMember("player")]
    public Player Player
    {
        get
        {
            CheckIfDisposed();
            return _player;
        }
    }

    public PlayerLoggedOutEvent(Player player)
    {
        _player = player;
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
