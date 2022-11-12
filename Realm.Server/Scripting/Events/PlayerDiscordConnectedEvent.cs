namespace Realm.Server.Scripting.Events;

public class PlayerDiscordConnectedEvent : INamedLuaEvent, IDisposable
{
    private bool _disposed = false;
    private readonly RPGPlayer _player;
    private readonly DiscordUser _discordUser;

    public static string EventName => "onPlayerDiscordConnected";

    public RPGPlayer Player
    {
        get
        {
            CheckIfDisposed();
            return _player;
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

    public PlayerDiscordConnectedEvent(RPGPlayer player, DiscordUser discordUser)
    {
        _player = player;
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
