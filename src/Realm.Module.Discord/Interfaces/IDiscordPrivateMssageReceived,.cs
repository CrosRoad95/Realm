namespace Realm.Module.Discord.Interfaces;

public interface IDiscordPrivateMessageReceived
{
    void HandlePrivateMessage(ulong userId, ulong messageId, string content);
}
