namespace Realm.Server.Scripting.Events;

[NoDefaultScriptAccess]
public class PlayerSpawnedEvent : INamedLuaEvent, IDisposable
{
    private bool _disposed = false;
    private readonly Player _player;
    private readonly Vector3 _position;

    public static string EventName => "onPlayerSpawn";

    [ScriptMember("player")]
    public Player Player

    {
        get
        {
            CheckIfDisposed();
            return _player;
        }
    }

    [ScriptMember("position")]
    public Vector3 Position
    {
        get
        {
            CheckIfDisposed();
            return _position;
        }
    }

    public PlayerSpawnedEvent(Player player, Vector3 position)
    {
        _player = player;
        _position = position;
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
