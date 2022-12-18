using Realm.Domain.Components.Players;

namespace Realm.Server.Scripting.Events;

[NoDefaultScriptAccess]
public class PlayerLoggedInEvent : INamedLuaEvent, IDisposable
{
    private bool _disposed = false;
    private readonly AccountComponent _accountComponent;

    public static string EventName => "onPlayerLogin";

    [ScriptMember("accountComponent")]
    public AccountComponent AccountComponent
    {
        get
        {
            CheckIfDisposed();
            return _accountComponent;
        }
    }

    public PlayerLoggedInEvent(AccountComponent accountComponent)
    {
        _accountComponent = accountComponent;
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
