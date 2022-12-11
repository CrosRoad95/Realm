namespace Realm.Server.Scripting.Events;

[NoDefaultScriptAccess]
public class PlayerJoinedEvent : INamedLuaEvent, IDisposable
{
    private bool _disposed = false;
    private readonly RPGPlayer _rpgPlayer;

    public static string EventName => "onPlayerJoin";

    [ScriptMember("player")]
    public RPGPlayer RPGPlayer
    {
        get
        {
            CheckIfDisposed();
            return _rpgPlayer;
        }
    }

    public PlayerJoinedEvent(RPGPlayer rpgPlayer)
    {
        _rpgPlayer = rpgPlayer;
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
