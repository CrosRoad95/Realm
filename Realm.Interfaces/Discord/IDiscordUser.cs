namespace Realm.Interfaces.Discord;

public interface IDiscordUser
{
    string Username { get; }

    Task SendTextMessage(string text);
}
