namespace Realm.Module.Discord.Interfaces;

public interface IDiscordTextBasedCommandHandler
{
    Task HandleTextCommand(ulong userId, ulong messageId, string command);
}
