namespace Realm.Server.Scripting.Events;

[NoDefaultScriptAccess]
public class ServerReloadedEvent : INamedLuaEvent, IDisposable
{
    private bool _disposed = false;

    public static string EventName => "onServerReloaded";

    public ServerReloadedEvent()
    {
    }

    [NoScriptAccess]
    public void Dispose()
    {
        _disposed = true;
    }
}
