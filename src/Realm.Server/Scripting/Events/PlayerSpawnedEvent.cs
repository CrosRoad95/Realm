namespace Realm.Server.Scripting.Events;

public class PlayerSpawnedEvent : INamedLuaEvent, IDisposable
{
    private bool _disposed = false;
    private readonly RPGPlayer _rpgPlayer;
    private readonly RPGSpawn? _spawn;
    private readonly Vector3 _position;

    public static string EventName => "onPlayerSpawn";

    public RPGPlayer rpgPlayer
    {
        get
        {
            CheckIfDisposed();
            return _rpgPlayer;
        }
    }

    public RPGSpawn? Spawn
    {
        get
        {
            CheckIfDisposed();
            return _spawn;
        }
    }

    public Vector3 Position
    {
        get
        {
            CheckIfDisposed();
            return _position;
        }
    }

    public PlayerSpawnedEvent(RPGPlayer rpgPlayer, RPGSpawn spawn)
    {
        _rpgPlayer = rpgPlayer;
        _spawn = spawn;
    }
    
    public PlayerSpawnedEvent(RPGPlayer rpgPlayer, Vector3 position)
    {
        _rpgPlayer = rpgPlayer;
        _position = position;
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
