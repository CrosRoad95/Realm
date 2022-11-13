namespace Realm.Interfaces.Discord;

public interface IDiscordUser
{
    ulong Id { get; }
    string Username { get; }

    Task SendTextMessage(string text);
}
