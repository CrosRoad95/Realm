namespace Realm.Persistance.Scripting.Classes;

[NoDefaultScriptAccess]
public class DiscordUser : IDisposable
{
    private bool _disposed;
    private ulong _id = 0;
    private readonly IDiscord _discord;
    private IDiscordUser _user = default!;

    [ScriptMember("id")]
    public ulong Id
    {
        get
        {
            CheckIfDisposed();
            return _id;
        }
    }

    [ScriptMember("username")]
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

    public void InitializeById(ulong id)
    {
        CheckIfDisposed();

        _id = id;
        _user = _discord.GetGuild()?.GetUserById(id) ?? throw new Exception($"Failed to get discord by user id {id}");
    }

    [ScriptMember("sendTextMessage")]
    public void SendTextMessage(string text)
    {
        CheckIfDisposed();

        _user.SendTextMessage(text);
    }

    private void CheckIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }

    [ScriptMember("toString")]
    public override string ToString() => "";

    public void Dispose()
    {
        _disposed = true;
    }
}
