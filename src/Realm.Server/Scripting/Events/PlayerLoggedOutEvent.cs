namespace Realm.Server.Scripting.Events;

public class PlayerLoggedOutEvent : INamedLuaEvent, IDisposable
{
    private bool _disposed = false;
    private readonly RPGPlayer _rpgPlayer;

    public static string EventName => "onPlayerLogout";

    public RPGPlayer rpgPlayer
    {
        get
        {
            CheckIfDisposed();
            return _rpgPlayer;
        }
    }

    public PlayerLoggedOutEvent(RPGPlayer rpgPlayer)
    {
        _rpgPlayer = rpgPlayer;
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
