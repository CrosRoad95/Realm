namespace Realm.Interfaces.Discord;

public interface IDiscordMessage
{
    Task Modify(string newContent);
}
