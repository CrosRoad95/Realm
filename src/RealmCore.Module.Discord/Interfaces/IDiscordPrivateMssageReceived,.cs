namespace RealmCore.Module.Discord.Interfaces;

public interface IDiscordPrivateMessageReceivedHandler : IDiscordHandler
{
    Task HandlePrivateMessage(ulong userId, ulong messageId, string content, CancellationToken cancellationToken);
}
