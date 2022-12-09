namespace Realm.Module.Discord.Scripting.Events;

public class DiscordStatusChannelUpdateContext : INamedLuaEvent
{
    public static string EventName => "onDiscordStatusChannelUpdate";
    private readonly StringBuilder _content = new StringBuilder();
    public string Content => _content.ToString();


    public DiscordStatusChannelUpdateContext()
    {

    }

    [ScriptMember("addLine")]
    public bool AddLine(string line)
    {
        _content.AppendLine(line);
        return true;
    }

    public override string ToString() => "DiscordStatusChannelUpdateContext";
}
