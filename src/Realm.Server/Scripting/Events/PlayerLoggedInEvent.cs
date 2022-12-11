namespace Realm.Server.Scripting.Events;

[NoDefaultScriptAccess]
public class PlayerLoggedInEvent : INamedLuaEvent, IDisposable
{
    private bool _disposed = false;
    private readonly RPGPlayer _rpgPlayer;
    private readonly PlayerAccount _account;

    public static string EventName => "onPlayerLogin";

    [ScriptMember("player")]
    public RPGPlayer RPGPlayer
    {
        get
        {
            CheckIfDisposed();
            return _rpgPlayer;
        }
    }

    [ScriptMember("account")]
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

    public void Dispose()
    {
        _disposed = true;
    }
}
