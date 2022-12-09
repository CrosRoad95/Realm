namespace Realm.Module.Discord.Scripting.Events;

public class DiscordUserChangedEvent : INamedLuaEvent, IDisposable
{
    private bool _disposed = false;
    private readonly PlayerAccount _account;
    private readonly DiscordUser _discordUser;

    public static string EventName => "onDiscordUserChange";

    public PlayerAccount Account
    {
        get
        {
            CheckIfDisposed();
            return _account;
        }
    }

    public DiscordUser Discord
    {
        get
        {
            CheckIfDisposed();
            return _discordUser;
        }
    }

    public DiscordUserChangedEvent(PlayerAccount account, DiscordUser discordUser)
    {
        _account = account;
        _discordUser = discordUser;
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
