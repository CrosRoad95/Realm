using Realm.Domain.Elements;

namespace Realm.Server.Scripting.Events;

public class PlayerLoggedInEvent : INamedLuaEvent, IDisposable
{
    private bool _disposed = false;
    private readonly RPGPlayer _player;
    private readonly PlayerAccount _account;

    public static string EventName => "onPlayerLogin";

    public RPGPlayer Player
    {
        get
        {
            CheckIfDisposed();
            return _player;
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

    public PlayerLoggedInEvent(RPGPlayer player, PlayerAccount account)
    {
        _player = player;
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
