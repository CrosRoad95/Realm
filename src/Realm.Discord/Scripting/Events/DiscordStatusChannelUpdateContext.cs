namespace Realm.Module.Discord.Scripting.Events;

[NoDefaultScriptAccess]
public class DiscordStatusChannelUpdateContext : INamedLuaEvent, IDisposable
{
    private bool _disposed = false;
    public static string EventName => "onDiscordStatusChannelUpdate";
    private readonly StringBuilder _content = new StringBuilder();
    public string Content
    {
        get
        {
            CheckIfDisposed();
            return _content.ToString();
        }
    }

    public DiscordStatusChannelUpdateContext()
    {

    }

    [ScriptMember("addLine")]
    public bool AddLine(string line)
    {
        CheckIfDisposed();
        _content.AppendLine(line);
        return true;
    }

    public override string ToString() => "DiscordStatusChannelUpdateContext";

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
