namespace Realm.Server.Scripting.Events;

[NoDefaultScriptAccess]
public class PlayerJoinedEvent : INamedLuaEvent, IDisposable
{
    private bool _disposed = false;
    private readonly Entity _entity;

    public static string EventName => "onPlayerJoin";

    [ScriptMember("entity")]
    public Entity Entity
    {
        get
        {
            CheckIfDisposed();
            return _entity;
        }
    }

    public PlayerJoinedEvent(Entity entity)
    {
        _entity = entity;
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
