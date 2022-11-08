namespace Realm.Server.Scripting.Events;

public class PlayerSpawnedEvent : INamedLuaEvent, IDisposable
{
    private bool _disposed = false;
    private RPGPlayer _player;
    private Spawn _spawn;

    public static string EventName => "onPlayerSpawn";

    public RPGPlayer Player
    {
        get
        {
            CheckIfDisposed();
            return _player;
        }
    }

    public Spawn Spawn
    {
        get
        {
            CheckIfDisposed();
            return _spawn;
        }
    }

    public PlayerSpawnedEvent(RPGPlayer player, Spawn spawn)
    {
        _player = player;
        _spawn = spawn;
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
