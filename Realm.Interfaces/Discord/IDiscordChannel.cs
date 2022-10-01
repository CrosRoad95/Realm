namespace Realm.Interfaces.Discord;

public interface IDiscordChannel
{
    Task<IDiscordMessage?> GetLastMessageSendByUser(ulong userId);
    Task<IDiscordMessage> SendMessage(string message);
}
