using SlipeServer.Server.Elements;

namespace Realm.Module.Discord.Scripting.Events;

public class DiscordPlayerConnectedEvent : INamedLuaEvent, IDisposable
{
    private bool _disposed = false;
    private readonly Player _rpgPlayer;
    private readonly DiscordUser _discordUser;

    public static string EventName => "onPlayerDiscordConnected";

    public Player player
    {
        get
        {
            CheckIfDisposed();
            return _rpgPlayer;
        }
    }

    public DiscordUser DiscordUser
    {
        get
        {
            CheckIfDisposed();
            return _discordUser;
        }
    }

    public DiscordPlayerConnectedEvent(Player player, DiscordUser discordUser)
    {
        _rpgPlayer = player;
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
