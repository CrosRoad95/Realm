namespace RealmCore.Module.Discord.Interfaces;

public interface IDiscordTextBasedCommandHandler : IDiscordHandler
{
    Task HandleTextCommand(ulong userId, ulong messageId, string command, CancellationToken cancellationToken);
}
