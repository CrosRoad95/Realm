namespace Realm.Scripting.Classes.Contextes;

public class DiscordStatusChannelUpdateContext
{
    private readonly StringBuilder _content = new StringBuilder();
    public string Content => _content.ToString();

    public DiscordStatusChannelUpdateContext()
    {

    }

    public bool AddLine(string line)
    {
        _content.AppendLine(line);
        return true;
    }

    public override string ToString() => "DiscordStatusChannelUpdateContext";
}
