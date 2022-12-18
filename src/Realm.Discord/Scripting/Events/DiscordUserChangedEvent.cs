using Realm.Domain.Components.Players;

namespace Realm.Module.Discord.Scripting.Events;

[NoDefaultScriptAccess]
public class DiscordUserChangedEvent : INamedLuaEvent, IDisposable
{
    private bool _disposed = false;
    private readonly DiscordComponent _discordComponent;

    public static string EventName => "onDiscordUserChange";

    [ScriptMember("discordComponent")]
    public DiscordComponent DiscordComponent
    {
        get
        {
            CheckIfDisposed();
            return _discordComponent;
        }
    }

    public DiscordUserChangedEvent(DiscordComponent discordComponent)
    {
        _discordComponent = discordComponent;
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
