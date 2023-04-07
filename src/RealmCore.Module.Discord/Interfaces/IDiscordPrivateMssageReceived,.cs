namespace RealmCore.Module.Discord.Interfaces;

public interface IDiscordPrivateMessageReceived
{
    Task HandlePrivateMessage(ulong userId, ulong messageId, string content);
}
