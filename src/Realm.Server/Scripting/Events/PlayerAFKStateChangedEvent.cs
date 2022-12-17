namespace Realm.Server.Scripting.Events;

[NoDefaultScriptAccess]
public class PlayerAFKStateChangedEvent : INamedLuaEvent, IDisposable
{
    private bool _disposed = false;
    private readonly Player _rpgPlayer;
    private readonly bool _afk;

    public static string EventName => "onPlayerAFKStateChanged";

    [ScriptMember("player")]
    public Player Player
    {
        get
        {
            CheckIfDisposed();
            return _rpgPlayer;
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

    public PlayerAFKStateChangedEvent(Player player, bool afk)
    {
        _rpgPlayer = player;
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
