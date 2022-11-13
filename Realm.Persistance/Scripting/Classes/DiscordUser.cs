namespace Realm.Persistance.Scripting.Classes;

public class DiscordUser : IDisposable
{
    private bool _disposed;
    private ulong _id = 0;
    private readonly IDiscord _discord;
    private IDiscordUser _user = default!;

    public ulong Id
    {
        get
        {
            CheckIfDisposed();
            return _id;
        }
    }

    public string Username
    {
        get
        {
            CheckIfDisposed();
            return _user.Username;
        }
    }

    public DiscordUser(IDiscord discord)
    {
        _discord = discord;
    }

    [NoScriptAccess]
    public void InitializeById(ulong id)
    {
        CheckIfDisposed();

        _id = id;
        _user = _discord.GetGuild()?.GetUserById(id) ?? throw new Exception($"Failed to get discord by user id {id}");
    }

    public void SendTextMessage(string text)
    {
        CheckIfDisposed();

        _user.SendTextMessage(text);
    }

    [NoScriptAccess]
    private void CheckIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }

    public string LongUserFriendlyName() => ToString();
    public override string ToString() => "";

    [NoScriptAccess]
    public void Dispose()
    {
        _disposed = true;
    }
}
