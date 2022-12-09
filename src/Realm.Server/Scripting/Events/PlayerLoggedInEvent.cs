namespace Realm.Server.Scripting.Events;

public class PlayerLoggedInEvent : INamedLuaEvent, IDisposable
{
    private bool _disposed = false;
    private readonly RPGPlayer _rpgPlayer;
    private readonly PlayerAccount _account;

    public static string EventName => "onPlayerLogin";

    public RPGPlayer rpgPlayer
    {
        get
        {
            CheckIfDisposed();
            return _rpgPlayer;
        }
    }

    public PlayerAccount Account
    {
        get
        {
            CheckIfDisposed();
            return _account;
        }
    }

    public PlayerLoggedInEvent(RPGPlayer rpgPlayer, PlayerAccount account)
    {
        _rpgPlayer = rpgPlayer;
        _account = account;
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
